using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour, IObstacle
{
	static float laserIntersectionThreshold = 0.1f;
	static float maxLaserLength = 10f;
	static float laserSpeed = 1.0f;

	public GameObject laserHitEffect;
	public Color color;
	public IObstacle hittedObstacle;
	public GameObject hittedObstacleGO;
	public List<Laser> parents, children;
	public bool IsDrawing { get; private set; }

	LineRenderer lineRenderer;
	CapsuleCollider capsuleCollider;
	Coroutine laserRaycastCoroutine, laserDrawCoroutine;

	public Action OnObjectChanged { get; set; }

	private void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
		capsuleCollider = GetComponent<CapsuleCollider>();
	}

	public void Init(Color _color, Vector3 _startPos, Vector3 _direction, Laser[] _parents)
	{
		lineRenderer.endColor = lineRenderer.startColor = color = _color;
		ParticleSystem.MainModule particleMain = laserHitEffect.GetComponent<ParticleSystem>().main;
		particleMain.startColor = color;

		parents = new List<Laser>();
		children = new List<Laser>();
		if (_parents != null)
		{
			for (int i = 0; i < _parents.Length; i++)
				parents.Add(_parents[i]);
		}

		capsuleCollider.enabled = true;
		SetStartPos(_startPos);
		SetDestinationPos(_startPos + _direction * 0.1f);
		DrawLaser(true, true);
	}

	public void AddChild(Laser child)
	{
		children.Add(child);
	}

	private void SetStartPos(Vector3 pos)
	{
		transform.position = pos;
		lineRenderer.SetPosition(0, pos);
		lineRenderer.SetPosition(1, pos);
	}

	private void SetEndPos(Vector3 pos)
	{
		SetDestinationPos(pos);
		if (laserRaycastCoroutine != null)
			StopCoroutine(laserRaycastCoroutine);
		if (laserDrawCoroutine != null)
			StopCoroutine(laserDrawCoroutine);
		lineRenderer.SetPosition(1, pos);
	}

	private void SetDestinationPos(Vector3 pos)
	{
		transform.forward = pos - transform.position;
		float length = (pos - transform.position).magnitude;
		capsuleCollider.height = length;
		capsuleCollider.center = new Vector3(0f, 0f, length * 0.5f);
	}

	public Vector3 GetStartPos()
	{
		return lineRenderer.GetPosition(0);
	}

	public Vector3 GetEndPos()
	{
		return lineRenderer.GetPosition(1);
	}

	private RaycastHit[] RaycastLaser()
	{
		gameObject.layer = 2; // Ignore self in raycast.
		ChangeChildrenRaycastLayer(2, true); // Ignore children in raycast.
		ChangeParentsRaycastLayer(2, false); // Ignore direct parents in raycast.

		int layerMask = 1 << LayerMask.NameToLayer("Obstacle") | 1 << LayerMask.NameToLayer("Laser");
		RaycastHit[] hit = Physics.RaycastAll(transform.position, transform.forward, maxLaserLength, layerMask);
		// ReEnable.
		gameObject.layer = 9;
		ChangeChildrenRaycastLayer(9, true);
		ChangeParentsRaycastLayer(9, false);
		return hit;
	}

	private void DrawLaser(bool forceDraw = false, bool waitForFixedUpdate = false)
	{
		if (laserRaycastCoroutine != null)
			StopCoroutine(laserRaycastCoroutine);
		laserRaycastCoroutine = StartCoroutine(DrawLaserByRaycastCoroutine(forceDraw, waitForFixedUpdate));
	}

	private IEnumerator DrawLaserByRaycastCoroutine(bool forceDraw = false, bool waitForFixedUpdate = false)
	{
		if (waitForFixedUpdate)
			yield return new WaitForFixedUpdate();

		RaycastHit[] hits = RaycastLaser();
		IObstacle obstacle;
		Vector3 hitPoint, hitNormal;
		List<Laser> dirtyLasers;
		CalcDestination(hits, out obstacle, out hitPoint, out hitNormal, out dirtyLasers);
		if (forceDraw || hittedObstacle != obstacle)
		{
			SetDestinationPos(hitPoint);
			// Redraw dirty lasers.
			if (dirtyLasers.Count > 0)
			{
				foreach (Laser laser in dirtyLasers)
					laser.DrawLaser(false, true);
			}

			if (laserDrawCoroutine != null)
				StopCoroutine(laserDrawCoroutine);
			laserDrawCoroutine = StartCoroutine(DrawLaserByObstacleCoroutine(obstacle, hitPoint, hitNormal));
		}
	}

	private void OrderHitsByDistance(ref RaycastHit[] hits)
	{
		Array.Sort(hits, delegate (RaycastHit hit1, RaycastHit hit2)
		{
			return hit1.distance.CompareTo(hit2.distance);
		});
	}

	private void CalcDestination(RaycastHit[] hits, out IObstacle obstacle, out Vector3 hitPoint, out Vector3 hitNormal, out List<Laser> dirtyLasers)
	{
		OrderHitsByDistance(ref hits);

		dirtyLasers = new List<Laser>();
		foreach (RaycastHit hit in hits)
		{
			obstacle = hit.collider ? hit.collider.GetComponentInParent<IObstacle>() : null;
			if (obstacle == null)
				continue;

			// Calculating laser hit position with obstacle.
			obstacle.CalcLaserHitPos(this, hit.point, hit.normal, out hitPoint, out hitNormal);
			hitNormal = hit.normal;

			Laser laser = obstacle as Laser;
			if (laser != null)
			{
				Vector3 newDirection = transform.forward + laser.transform.forward;
				if (newDirection.magnitude < laserIntersectionThreshold)
					return;
			}

			if (laser != null && laser.IsDrawing/* && hitNormal.magnitude >= laserIntersectionThreshold*/)
			{
				Vector3 otherStartPos = laser.GetStartPos();
				Vector3 otherEndPos = laser.GetEndPos();
				float otherMagnitude = Vector3.Distance(otherStartPos, otherEndPos);
				float otherDistToHitPoint = Vector3.Distance(otherStartPos, hitPoint);
				// Checking if hit point is on other laser.
				if (otherMagnitude >= otherDistToHitPoint)
					return;
				else
				{
					float thisDistance = Vector3.Distance(GetEndPos(), hitPoint);
					float otherDistance = Vector3.Distance(otherEndPos, hitPoint);
					// If this laser's distance to hit point is less then other laser's, then set other laser dirty to redraw it.
					if (thisDistance < otherDistance)
					{
						dirtyLasers.Add(laser);
						continue;
					}
					else return;
				}
			}
			else return;
		}

		obstacle = null;
		hitPoint = transform.position + transform.forward * maxLaserLength;
		hitNormal = Vector3.zero;
		return;
	}

	private IEnumerator DrawLaserByObstacleCoroutine(IObstacle obstacle, Vector3 hitPoint, Vector3 hitNormal)
	{
		Debug.Log("Drawing: " + color.ToString());
		IsDrawing = true;
		// Remove any children, if there are.
		RemoveChildren();
		// Deleting reference from old hitted obstacle.
		SetHittedObstacle(null);
		
		// Setting new hitted obstacle.
		SetHittedObstacle(obstacle);
		// Actually drawing the laser.
		yield return DrawLaserCoroutine(hitPoint);
		IsDrawing = false;
		if (obstacle != null)
		{
			Debug.DrawLine(hitPoint, hitPoint + hitNormal, Color.red, 10f);

			SetHittedObstacle(null);
			// Notify the obstacle, that this laser hitted him.
			obstacle.OnLaserHitted(this, hitPoint, hitNormal);

			SetHittedObstacle(obstacle);
		}
	}

	private IEnumerator DrawLaserCoroutine(Vector3 destination)
	{
		Vector3 lineStartPos = lineRenderer.GetPosition(0);
		Vector3 lineEndPos = lineRenderer.GetPosition(1);
		float lineLength = Vector3.Distance(lineStartPos, lineEndPos);
		float lineTargetLength = Vector3.Distance(lineStartPos, destination);

		Vector3 velocity = transform.forward * laserSpeed;
		while (lineLength < lineTargetLength)
		{
			lineEndPos += velocity * Time.deltaTime;
			lineRenderer.SetPosition(1, lineEndPos);
			lineLength = Vector3.Distance(lineStartPos, lineEndPos);
			yield return null; // Wait one frame.
		}
		lineRenderer.SetPosition(1, destination);
	}

	public void SetHittedObstacle(IObstacle obstacle)
	{
		if (hittedObstacle != null)
			hittedObstacle.OnObjectChanged -= OnObstacleChanged;
		if (obstacle != null)
			obstacle.OnObjectChanged += OnObstacleChanged;

		hittedObstacle = obstacle;
		MonoBehaviour mb = obstacle as MonoBehaviour;
		if (mb != null)
			hittedObstacleGO = mb.gameObject;
		else
			hittedObstacleGO = null;
	}

	public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
	{
		Vector3 lineVec3 = linePoint2 - linePoint1;
		Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
		Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

		float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

		//is coplanar, and not parrallel
		if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
		{
			float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
			intersection = linePoint1 + (lineVec1 * s);
			return true;
		}
		else
		{
			intersection = Vector3.zero;
			return false;
		}
	}

	public void CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut)
	{
		Vector3 newDirection = transform.forward + hittedLaser.transform.forward;
		if (newDirection.magnitude < laserIntersectionThreshold)
		{
			hitPointOut = (GetEndPos() + hittedLaser.GetEndPos()) / 2f;
			hitNormalOut = newDirection;
			return;
		}
		// Finding Intersection Point.
		LineLineIntersection(out Vector3 intersection, transform.position, transform.forward, hittedLaser.transform.position, hittedLaser.transform.forward);
		hitPointOut = intersection;
		hitNormalOut = hitNormal;
	}

	public void OnLaserHitted(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		RemoveChildren();
		SetEndPos(hitPoint);
		// Setting new hitted Laser to hitted Obstacle.
		SetHittedObstacle(hittedLaser);
		// Let registered objects know, that this laser changed.
		OnObjectChanged?.Invoke();

		// Creation of new Laser.
		Vector3 newDirection = (transform.forward + hittedLaser.transform.forward).normalized;
		if (newDirection.magnitude > laserIntersectionThreshold)
		{
			Color newColor = color + hittedLaser.color;
			newColor.a = 1f;
			Laser[] parents = { this, hittedLaser };
			GameManager.Instance.gameMap.SpawnLaser(newColor, hitPoint/* + newDirection * 1f*/, newDirection, parents);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Laser laser = other.GetComponentInParent<Laser>();
		if (laser != null) // Ignore if we collided with laser.
		{
			/*Debug.Log("OnTriggerEnter: LASER");
			Vector3 newDirection = (transform.forward + laser.transform.forward).normalized;
			if (newDirection.magnitude < laserIntersectionThreshold)
			{
				Vector3 hitPos = (GetEndPos() + laser.GetEndPos()) / 2f;
				SetDestinationPos(hitPos);
				laser.SetDestinationPos(hitPos);
				DrawLaser(false, true);
				// laser.DrawLaser(false, true);
				Debug.Log("OnTriggerEnter: LASERRRRRRRRRRRRRRRRRRRRRRRR");
			}*/
			return;
		}

		IObstacle obstacle = other.GetComponentInParent<IObstacle>();
		if (obstacle == null || hittedObstacle == obstacle// || IsChild(laser) || IsParent(laser)
			|| IsHittedObstacleOfParent(obstacle))
			return;

		Debug.Log("OnTriggerEnter: " + color.ToString() + ", Other: " + other.transform.parent.name);

		DrawLaser(false, true);
		// Let registered objects know, that this laser changed.
		OnObjectChanged?.Invoke();
	}

	private void OnObstacleChanged()
	{
		if (!gameObject.activeInHierarchy)
			return;

		Debug.Log("OnObstacleChanged: " + color.ToString());
		RemoveChildren();
		DrawLaser(true, true);
	}

	private void ChangeChildrenRaycastLayer(int newLayer, bool isRecursive)
	{
		if (children.Count == 0)
			return;

		for (int i = children.Count - 1; i >= 0; i--)
		{
			if (children[i] == null)
			{
				children.RemoveAt(i);
				continue;
			}
			children[i].gameObject.layer = newLayer;
			if (isRecursive)
				children[i].ChangeChildrenRaycastLayer(newLayer, isRecursive);
		}
	}

	private void ChangeParentsRaycastLayer(int newLayer, bool isRecursive)
	{
		if (parents.Count == 0)
			return;

		for (int i = parents.Count - 1; i >= 0; i--)
		{
			if (parents[i] == null)
			{
				parents.RemoveAt(i);
				continue;
			}
			parents[i].gameObject.layer = newLayer;
			if (isRecursive)
				parents[i].ChangeParentsRaycastLayer(newLayer, isRecursive);
		}
	}

	private bool IsChild(Laser child)
	{
		if (children.Count == 0)
			return false;

		for (int i = children.Count - 1; i >= 0; i--)
		{
			if (children[i] == null)
			{
				children.RemoveAt(i);
				continue;
			}
			if (child == children[i])
				return true;
			if (children[i].IsChild(child))
				return true;
			else continue;
		}
		return false;
	}

	private bool IsParent(Laser parent)
	{
		if (parents.Count == 0)
			return false;

		for (int i = parents.Count - 1; i >= 0; i--)
		{
			if (parents[i] == null)
			{
				parents.RemoveAt(i);
				continue;
			}
			if (parent == parents[i])
				return true;
			if (parents[i].IsParent(parent))
				return true;
			else continue;
		}
		return false;
	}

	private bool IsHittedObstacleOfParent(IObstacle obstacle)
	{
		if (parents.Count == 0)
			return false;

		for (int i = parents.Count - 1; i >= 0; i--)
		{
			if (parents[i] == null)
			{
				parents.RemoveAt(i);
				continue;
			}
			if (parents[i].hittedObstacle == obstacle)
				return true;
		}
		return false;
	}

	private void RemoveChildren()
	{
		if (children.Count == 0)
			return;

		for (int i = children.Count - 1; i >= 0; i--)
		{
			if (children[i] == null)
				continue;
			
			children[i].gameObject.layer = 2;
			children[i].capsuleCollider.enabled = false;
			Destroy(children[i].gameObject);
			children[i].RemoveChildren();
		}
		children.Clear();
	}

	private void EnableHitEffects()
	{
		laserHitEffect.SetActive(true);
		laserHitEffect.transform.position = lineRenderer.GetPosition(0);
	}

	private void DisableHitEffects()
	{
		// laserHitEffect.SetActive(false);
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		SetHittedObstacle(null);
		OnObjectChanged?.Invoke();
	}
}

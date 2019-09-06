using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour, IObstacle
{
	static float laserIntersectionThreshold = 0.1f;
	static float maxLaserLength = 10f;
	static float laserSpeed = 2.0f;

	public GameObject laserHitEffect;
	public Color color;
	public IObstacle hittedObstacle;
	public List<Laser> parents, children;
	public bool IsDrawing { get; private set; }

	LineRenderer lineRenderer;
	CapsuleCollider capsuleCollider;
	Coroutine laserDrawCoroutine;

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
		SetDestinationPos(_startPos + _direction * maxLaserLength);
		DrawLaser(true);
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

	public Vector3 GetEndPos()
	{
		return lineRenderer.GetPosition(1);
	}

	private void RaycastLaser(out RaycastHit hit)
	{
		gameObject.layer = 2; // Ignore self in raycast.
		ChangeChildrenRaycastLayer(2, true); // Ignore children in raycast.
		ChangeParentsRaycastLayer(2, false); // Ignore direct parents in raycast.

		int layerMask = 1 << LayerMask.NameToLayer("Obstacle") | 1 << LayerMask.NameToLayer("Laser");
		Physics.Raycast(transform.position, transform.forward, out hit, maxLaserLength, layerMask);
		// ReEnable.
		gameObject.layer = 9;
		ChangeChildrenRaycastLayer(9, true);
		ChangeParentsRaycastLayer(9, false);
	}

	private void DrawLaser(bool forceDraw = false, bool waitForFixedUpdate = false)
	{
		if (laserDrawCoroutine != null)
			StopCoroutine(laserDrawCoroutine);
		laserDrawCoroutine = StartCoroutine(DrawLaserByRaycastCoroutine(forceDraw, waitForFixedUpdate));
	}

	private IEnumerator DrawLaserByRaycastCoroutine(bool forceDraw = false, bool waitForFixedUpdate = false)
	{
		if (waitForFixedUpdate)
			yield return new WaitForFixedUpdate();

		RaycastHit hit;
		RaycastLaser(out hit);

		IObstacle obstacle = hit.collider ? hit.collider.GetComponentInParent<IObstacle>() : null;
		if (forceDraw || hittedObstacle != obstacle)
			yield return DrawLaserByObstacleCoroutine(obstacle, hit.point, hit.normal);
	}

	private IEnumerator DrawLaserByObstacleCoroutine(IObstacle obstacle, Vector3 hitPoint, Vector3 hitNormal)
	{
		IsDrawing = true;
		// Deleting reference from old hitted obstacle.
		SetHittedObstacle(null);
		// Remove any children, if there are.
		RemoveChildren();
		// Let registered objects know, that this laser changed.
		OnObjectChanged?.Invoke();

		Laser laser = (obstacle != null) ? (obstacle as Laser) : null;
		Vector3 endPos = transform.position + transform.forward * maxLaserLength;
		if (obstacle != null)
		{
			// Calculating laser hit position with obstacle.
			Vector3 hitPos = obstacle.CalcLaserHitPos(this, hitPoint, hitNormal);
			if (laser != null && laser.IsDrawing)
			{
				float thisDistance = Vector3.Distance(GetEndPos(), hitPos);
				float otherDistance = Vector3.Distance(laser.GetEndPos(), hitPos);
				// If this laser's distance to hit point is less then other laser's, then call draw function in other laser.
				if (thisDistance < otherDistance)
				{
					laser.DrawLaser();
					obstacle = laser = null;
				}
				else
					endPos = hitPos;
			}
			 else // If hitted obstacle is not laser.
		 		endPos = hitPos;
		}

		SetDestinationPos(endPos);
		// Setting new hitted obstacle.
		SetHittedObstacle(obstacle);
		// Actually drawing the laser.
		yield return DrawLaserCoroutine(endPos);
		IsDrawing = false;
		if (obstacle != null)
		{
			Debug.DrawLine(endPos, endPos + hitNormal, Color.red, 10f);

			SetHittedObstacle(null);

			// Notify the obstacle, that this laser hitted him.
			obstacle.OnLaserHitted(this, endPos, hitNormal);

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

	public Vector3 CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		// Finding Intersection Point.
		LineLineIntersection(out Vector3 intersection, transform.position, transform.forward, hittedLaser.transform.position, hittedLaser.transform.forward);
		return intersection;
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
			GameManager.Instance.gameMap.SpawnLaser(newColor, hitPoint/* + newDirection * 3f*/, newDirection, parents);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Laser laser = other.GetComponentInParent<Laser>();
		if (laser != null) // Ignore if we collided with laser.
		{
			Debug.Log("OnTriggerEnter: LASER");
			Vector3 newDirection = (transform.forward + laser.transform.forward).normalized;
			if (newDirection.magnitude < laserIntersectionThreshold)
			{
				Vector3 hitPos = (GetEndPos() + laser.GetEndPos()) / 2f;
				SetDestinationPos(hitPos);
				laser.SetDestinationPos(hitPos);
				DrawLaser(false, true);
				// laser.DrawLaser(false, true);
				Debug.Log("OnTriggerEnter: LASERRRRRRRRRRRRRRRRRRRRRRRR");
			}
			return;
		}

		IObstacle obstacle = other.GetComponentInParent<IObstacle>();
		if (obstacle == null || hittedObstacle == obstacle// || IsChild(laser) || IsParent(laser)
			|| IsHittedObstacleOfParent(obstacle))
			return;

		Debug.Log("OnTriggerEnter: " + color.ToString() + ", Other: " + other.transform.parent.name);

		DrawLaser();
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

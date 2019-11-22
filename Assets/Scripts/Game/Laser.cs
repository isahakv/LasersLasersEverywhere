using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour, IObstacle
{
	public static float laserParallelThreshold = 0.1f;
	static float maxLaserLength = 30f;
	static float laserSpeed = 5f;

	public Color color;
	public AnimationCurve colorAlphaCurve;
	public ParticleSystem[] laserHitEffects;
	public GameObject hittedObstacleGO; // For debugging.
	public List<Laser> parents, children;
	public bool IsDrawing { get; private set; }

	IObstacle causerObstacle, hittedObstacle;
	/** Components */
	LineRenderer lineRenderer;
	CapsuleCollider capsuleCollider;
	Material laserMaterial;
	/** Coroutines */
	Coroutine laserRaycastCoroutine, laserDrawCoroutine;

	public Action OnObjectChanged { get; set; }

	private void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
		capsuleCollider = GetComponent<CapsuleCollider>();
		laserMaterial = GetComponent<Renderer>().material;
	}

	private void Update()
	{
		color.a = colorAlphaCurve.Evaluate(Time.time);
		laserMaterial.SetColor("_TintColor", color);
	}

	public void Init(Color _color, Vector3 _startPos, Vector3 _direction, IObstacle _causerObstacle, Laser[] _parents)
	{
		lineRenderer.endColor = lineRenderer.startColor = color = _color;
		foreach (ParticleSystem effect in laserHitEffects)
		{
			ParticleSystem.MainModule particleMain = effect.main;
			particleMain.startColor = color;
		}
		

		causerObstacle = _causerObstacle;
		parents = new List<Laser>();
		children = new List<Laser>();
		if (_parents != null)
		{
			for (int i = 0; i < _parents.Length; i++)
				parents.Add(_parents[i]);
		}

		capsuleCollider.enabled = true;
		SetStartPos(_startPos);
		SetDestinationPos(_startPos + _direction * 0.1f, true);
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

	private void SetDestinationPos(Vector3 pos, bool changeDirection = false)
	{
		if (changeDirection)
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

	private void OrderHitsByDistance(ref RaycastHit[] hits)
	{
		Array.Sort(hits, (RaycastHit hit1, RaycastHit hit2) =>
		{
			return hit1.distance.CompareTo(hit2.distance);
		});
	}

	private RaycastHit[] RaycastLaser()
	{
		gameObject.layer = 2; // Ignore self in raycast.
		ChangeChildrenRaycastLayer(2, true); // Ignore children in raycast.
		ChangeParentsRaycastLayer(2, false); // Ignore direct parents in raycast.

		int layerMask = 1 << LayerMask.NameToLayer("Obstacle") | 1 << LayerMask.NameToLayer("Laser");
		RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, maxLaserLength, layerMask);
		// ReEnable.
		gameObject.layer = 9;
		ChangeChildrenRaycastLayer(9, true);
		ChangeParentsRaycastLayer(9, false);
		// Ordering hits by hit distance.
		OrderHitsByDistance(ref hits);
		return hits;
	}

	private void DrawLaser(bool forceDraw = false, bool waitForFixedUpdate = false)
	{
		if (laserRaycastCoroutine != null)
			StopCoroutine(laserRaycastCoroutine);
		laserRaycastCoroutine = StartCoroutine(DrawLaserByRaycastCoroutine(forceDraw, waitForFixedUpdate));

		//TODO:
		/*
		if (laserDrawCoroutine != null)
			StopCoroutine(laserDrawCoroutine);
		laserDrawCoroutine = StartCoroutine(DrawLaserByRaycastCoroutine(forceDraw, waitForFixedUpdate));
		*/
	}

	public void DrawLaser(IObstacle obstacle, Vector3 hitPoint, Vector3 hitNormal)
	{
		//TODO:
		/*
		if (laserDrawCoroutine != null)
			StopCoroutine(laserDrawCoroutine);
		laserDrawCoroutine = StartCoroutine(DrawLaserByObstacleCoroutine(obstacle, hitPoint, hitNormal));
		*/
	}

	private IEnumerator DrawLaserByRaycastCoroutine(bool forceDraw = false, bool waitForFixedUpdate = false)
	{
		if (waitForFixedUpdate)
			yield return new WaitForFixedUpdate();

		RaycastHit[] hits = RaycastLaser();
		CalcDestination(hits, out IObstacle obstacle, out Vector3 hitPoint, out Vector3 hitNormal);
		if (forceDraw || hittedObstacle != obstacle)
		{
			SetDestinationPos(hitPoint);

			if (laserDrawCoroutine != null)
				StopCoroutine(laserDrawCoroutine);
			laserDrawCoroutine = StartCoroutine(DrawLaserByObstacleCoroutine(obstacle, hitPoint, hitNormal));

			//TODO: yield return DrawLaserByObstacleCoroutine(obstacle, hitPoint, hitNormal);
		}
	}

	private void CalcDestination(RaycastHit[] hits, out IObstacle obstacle, out Vector3 hitPoint, out Vector3 hitNormal)
	{
		foreach (RaycastHit hit in hits)
		{
			obstacle = hit.collider ? hit.collider.GetComponentInParent<IObstacle>() : null;
			if (obstacle == null)
				continue;
			
			// Calculating laser hit position with obstacle.
			bool isBlock = obstacle.CalcLaserHitPos(this, hit.point, hit.normal, out hitPoint, out hitNormal);
			if (isBlock)
			{
				Debug.Log("CalcDestination: " + color.ToString());
				return;
			}
		}
		// If nothing blocking the laser.
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
		// Setting new hitted obstacle.
		SetHittedObstacle(obstacle);
		// Actually drawing the laser.
		yield return DrawLaserCoroutine(hitPoint);
		IsDrawing = false;
		if (obstacle != null)
		{
			Debug.DrawLine(hitPoint, hitPoint + hitNormal, Color.red, 10f);

			// SetHittedObstacle(null);
			// Notify the obstacle, that this laser hitted him.
			obstacle.OnLaserHitted(this, hitPoint, hitNormal);
			EnableHitEffects(hitNormal);

			// SetHittedObstacle(obstacle);
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

	public bool CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut)
	{
		// Checking if lasers coming parallel to each other.
		Vector3 newDirection = transform.forward + hittedLaser.transform.forward;
		if (newDirection.magnitude < laserParallelThreshold) // If they are parallel.
		{
			hitPointOut = (GetEndPos() + hittedLaser.GetEndPos()) / 2f;
			hitNormalOut = Vector3.zero; // newDirection;
			// TODO: Otimize in case of they are parallel.

			return true;
		}
		else // If not, than find they intersection point.
		{
			// Finding Intersection Point.
			LineLineIntersection(out Vector3 intersection, transform.position, transform.forward, hittedLaser.transform.position, hittedLaser.transform.forward);
			hitPointOut = intersection;
			hitNormalOut = hitNormal;
			// If hitted laser is already this laser's hitted obstacle, then don't block.
			if ((hittedLaser as IObstacle) == hittedObstacle) // TODO: Move this on top of this function.
				return false;

			if (IsDrawing)
			{
				float laserLength = Vector3.Distance(GetStartPos(), GetEndPos());
				float distToHitPoint = Vector3.Distance(GetStartPos(), hitPoint);
				// Checking if hit point is in this laser.
				if (laserLength >= distToHitPoint)
					return true;
				else // Else Checking if this laser is nearer to hitpoint then other.
				{
					float thisDistance = Vector3.Distance(GetEndPos(), hitPoint);
					float otherDistance = Vector3.Distance(hittedLaser.GetEndPos(), hitPoint);
					if (thisDistance <= otherDistance)
						return true;
					else
					{
						DrawLaser(false, true);
						return false;
					}
				}
			}
			return true;
		}
	}

	public void OnLaserHitted(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		if (IsChild(hittedLaser))
			return;

		// Disable hit effects
		DisableHitEffects();

		RemoveChildren();
		SetEndPos(hitPoint);
		// Setting new hitted Laser to hitted Obstacle.
		SetHittedObstacle(hittedLaser);

		// Ignore newly hitted laser.
		hittedLaser.SetHittedObstacle(null);
		// Let registered objects know, that this laser changed.
		OnObjectChanged?.Invoke();
		hittedLaser.SetHittedObstacle(this);

		Debug.Log("OnLaserHitted: " + color.ToString());

		// Creation of new Laser.
		if (hitNormal.magnitude > laserParallelThreshold)
		{
			Vector3 newDirection = (transform.forward + hittedLaser.transform.forward).normalized;
			Color newColor = color + hittedLaser.color;
			newColor.a = 1f;
			Laser[] parents = { this, hittedLaser };
			GameManager.Instance.gameMap.SpawnLaser(newColor, hitPoint, newDirection, null, parents);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Laser laser = other.GetComponentInParent<Laser>();
		if (laser != null) // Ignore if we collided with laser.
			return;

		IObstacle obstacle = other.GetComponentInParent<IObstacle>();
		if (obstacle == null || causerObstacle == obstacle// || IsChild(laser) || IsParent(laser)
			|| hittedObstacle == obstacle)
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
		DisableHitEffects();
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

	private void EnableHitEffects(Vector3 direction)
	{
		foreach (ParticleSystem effect in laserHitEffects)
		{
			effect.transform.position = lineRenderer.GetPosition(1);
			effect.transform.forward = direction;
			effect.gameObject.SetActive(true);
		}
	}

	private void DisableHitEffects()
	{
		foreach (ParticleSystem effect in laserHitEffects)
			effect.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		SetHittedObstacle(null);
		OnObjectChanged?.Invoke();
	}
}

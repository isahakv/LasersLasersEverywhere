using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSplitter : Item, IObstacle
{
	public Transform[] laserSpawnPoints;
	public MeshRenderer mesh;
	public float rotationSpeed = 100f;
	public Action OnObjectChanged { get; set; }

	bool isActive = false;
	Laser hittedLaser;
	Coroutine meshRotateCoroutine;

	public bool CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut)
	{
		Vector3 pos = transform.position;
		hitPointOut = new Vector3(pos.x, hitPoint.y, pos.z);
		hitNormalOut = hitNormal;
		return true;
	}

	public void OnLaserHitted(Laser _hittedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		//float cosine = Vector3.Dot(transform.forward, _hittedLaser.transform.forward);
		//if (Mathf.Approximately(cosine, 1f))
		hittedLaser = _hittedLaser;
		transform.forward = hittedLaser.transform.forward;
		hittedLaser.OnObjectChanged += OnLaserChanged;
		SetActiveState(true);

		for (int i = 0; i < laserSpawnPoints.Length; i++)
		{
			Laser[] parent = new Laser[] { hittedLaser };
			Vector3 pos = laserSpawnPoints[i].position;
			Vector3 dir = laserSpawnPoints[i].forward;
			GameManager.Instance.gameMap.SpawnLaser(hittedLaser.color, pos, dir, this, parent);
		}
	}

	private void OnLaserChanged()
	{
		hittedLaser.OnObjectChanged -= OnLaserChanged;
		hittedLaser = null;
		SetActiveState(false);
	}

	public override void RotateItem()
	{
		base.RotateItem();
		OnObjectChanged?.Invoke();
	}

	public override void RemoveItem()
	{
		base.RemoveItem();
		OnObjectChanged?.Invoke();
	}

	private void SetActiveState(bool _isActive)
	{
		isActive = _isActive;
		if (isActive)
		{
			Color color = hittedLaser.color;
			mesh.material.color = new Color(color.r, color.g, color.b, 0.8f);
			mesh.material.SetColor("_EmissionColor", 0.3f * color);
			if (meshRotateCoroutine == null)
				meshRotateCoroutine = StartCoroutine(MeshRotateCoroutine());
		}
		else
		{
			mesh.material.color = new Color(1f, 1f, 1f, 0.6f);
			mesh.material.SetColor("_EmissionColor", Color.black);
			if (meshRotateCoroutine != null)
			{
				StopCoroutine(meshRotateCoroutine);
				meshRotateCoroutine = null;
			}
		}
	}

	private IEnumerator MeshRotateCoroutine()
	{
		while(true)
		{
			mesh.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime, Space.Self);
			yield return null;
		}
	}
}

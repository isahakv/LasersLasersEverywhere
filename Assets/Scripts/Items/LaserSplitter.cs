using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSplitter : Item, IObstacle
{
	public Transform[] laserSpawnPoints;

	public Action OnObjectChanged { get; set; }

	public bool CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut)
	{
		hitPointOut = hitPoint;
		hitNormalOut = hitNormal;
		return true;
	}

	public void OnLaserHitted(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		float cosine = Vector3.Dot(transform.forward, hittedLaser.transform.forward);
		if (Mathf.Approximately(cosine, 1f))
		{
			for (int i = 0; i < laserSpawnPoints.Length; i++)
			{
				Laser[] parent = new Laser[] { hittedLaser };
				Vector3 pos = laserSpawnPoints[i].position;
				Vector3 dir = laserSpawnPoints[i].forward;
				GameManager.Instance.gameMap.SpawnLaser(hittedLaser.color, pos, dir, this, parent);
			}
		}
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
}

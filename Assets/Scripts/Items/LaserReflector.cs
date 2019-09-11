using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserReflector : Item, IObstacle
{
	public Action OnObjectChanged { get; set; }

	public bool CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut)
	{
		Vector3 newDirection = hittedLaser.transform.forward + hitNormal;
		if (newDirection.magnitude < Laser.laserParallelThreshold) // If there are parallel.
		{
			hitPointOut = hitPoint;
			hitNormalOut = hitNormal;
			return true;
		}

		Vector3 pos = transform.position;
		hitPointOut = new Vector3(pos.x, hitPoint.y, pos.z);
		hitNormalOut = hitNormal;
		return true;
	}

	public void OnLaserHitted(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		Vector3 newDirection = hittedLaser.transform.forward + hitNormal;
		if (newDirection.magnitude < Laser.laserParallelThreshold) // If there are parallel.
			return;

		Laser[] parent = { hittedLaser };
		Vector3 direction = Vector3.Reflect(hittedLaser.transform.forward, hitNormal);
		GameManager.Instance.gameMap.SpawnLaser(hittedLaser.color, hitPoint, direction, this, parent);
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

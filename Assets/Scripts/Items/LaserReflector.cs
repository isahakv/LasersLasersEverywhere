using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserReflector : Item, IObstacle
{
	public Action OnObjectChanged { get; set; }

	public Vector3 CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		return hitPoint;
	}

	public void OnLaserHitted(Laser hitedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		Laser[] parent = { hitedLaser };
		Vector3 direction = Vector3.Reflect(hitedLaser.transform.forward, hitNormal);
		GameManager.Instance.gameMap.SpawnLaser(hitedLaser.color, hitPoint, direction, parent);
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

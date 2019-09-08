using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserReflector : Item, IObstacle
{
	public Action OnObjectChanged { get; set; }

	public void CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut)
	{
		hitPointOut = hitPoint;
		hitNormalOut = Vector3.Reflect(hittedLaser.transform.forward, hitNormal);
	}

	public void OnLaserHitted(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		Laser[] parent = { hittedLaser };
		Vector3 direction = Vector3.Reflect(hittedLaser.transform.forward, hitNormal);
		GameManager.Instance.gameMap.SpawnLaser(hittedLaser.color, hitPoint, direction, parent);
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

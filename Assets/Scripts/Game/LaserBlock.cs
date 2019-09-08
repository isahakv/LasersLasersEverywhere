using System;
using UnityEngine;

public class LaserBlock : MonoBehaviour, IObstacle
{
	public Action OnObjectChanged { get; set; }

	public void CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut)
	{
		hitPointOut = hitPoint;
		hitNormalOut = hitNormal;
	}

	public void OnLaserHitted(Laser hitedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		// return new LaserCheckPoint(hitPoint, Vector3.zero, this, null);
	}
}

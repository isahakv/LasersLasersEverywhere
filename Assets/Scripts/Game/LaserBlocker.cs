using System;
using UnityEngine;

public class LaserBlocker : MonoBehaviour, IObstacle
{
	public Action OnObjectChanged { get; set; }

	public bool CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut)
	{
		hitPointOut = hitPoint;
		hitNormalOut = hitNormal;
		return true;
	}

	public void OnLaserHitted(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		// return new LaserCheckPoint(hitPoint, Vector3.zero, this, null);
	}
}

using System;
using UnityEngine;

public class LaserBlock : MonoBehaviour, IObstacle
{
	public Action OnObjectChanged { get; set; }

	public Vector3 CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		return hitPoint;
	}

	public void OnLaserHitted(Laser hitedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		// return new LaserCheckPoint(hitPoint, Vector3.zero, this, null);
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObstacle
{
	Action OnObjectChanged { get; set; }
	void CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut);
	void OnLaserHitted(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal);
}

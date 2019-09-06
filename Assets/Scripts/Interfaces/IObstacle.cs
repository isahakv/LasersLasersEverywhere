using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObstacle
{
	Action OnObjectChanged { get; set; }
	Vector3 CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal);
	void OnLaserHitted(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal);
}

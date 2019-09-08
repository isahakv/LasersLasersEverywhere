using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObstacle
{
	Action OnObjectChanged { get; set; }
	/// <summary>
	/// Calculates Laser hit against other obstacles.
	/// </summary>
	/// <param name="hittedLaser"></param>
	/// <param name="hitPoint"></param>
	/// <param name="hitNormal"></param>
	/// <param name="hitPointOut"></param>
	/// <param name="hitNormalOut"></param>
	/// <returns>Returns true if obstacle actually blocks the laser.</returns>
	bool CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut);
	void OnLaserHitted(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal);
}

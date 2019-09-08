using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeamer : Item, IObstacle
{
	public Color laserColor;
	public Action OnObjectChanged { get; set; }

	public override void SetColor(Color newColor)
	{
		laserColor = newColor;
		GetComponentInChildren<Renderer>().material.color = laserColor;
		GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", Color.black);
	}

	public void CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut)
	{
		hitPointOut = hitPoint;
		hitNormalOut = hitNormal;
	}

	public void OnLaserHitted(Laser hitedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{

	}
}

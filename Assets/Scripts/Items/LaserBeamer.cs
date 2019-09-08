using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeamer : Item, IObstacle
{
	public Color laserColor;
	public Action OnObjectChanged { get; set; }

	public override void SetColor(Color[] inputColors, Color[] outputColors)
	{
		laserColor = outputColors[0];
		GetComponentInChildren<Renderer>().material.color = laserColor;
		GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", Color.black);
	}

	public bool CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut)
	{
		hitPointOut = hitPoint;
		hitNormalOut = hitNormal;
		return true;
	}

	public void OnLaserHitted(Laser hitedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{

	}
}

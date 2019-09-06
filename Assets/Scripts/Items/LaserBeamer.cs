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

	public Vector3 CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		return hitPoint;
	}

	public void OnLaserHitted(Laser hitedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{

	}
}

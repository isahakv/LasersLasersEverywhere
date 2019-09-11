using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeamer : Item, IObstacle
{
	public MeshRenderer[] meshes;
	public Transform laserSpawnPos;
	public Color laserColor;
	public Action OnObjectChanged { get; set; }

	public override void SetColor(Color[] inputColors, Color[] outputColors)
	{
		laserColor = outputColors[0];

		foreach (MeshRenderer mesh in meshes)
		{
			mesh.material.SetColor("_Paintedcolor", laserColor);
			mesh.material.SetColor("_Tint", laserColor);
		}
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

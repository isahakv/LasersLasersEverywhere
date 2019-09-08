using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserObsorber : Item, IObstacle
{
	bool isActive = false;
	Laser laser;

	public Color requiredLaserColor;
	public Action OnObjectChanged { get; set; }
	public Action<bool> OnActiveStateChanged;

	public override void SetColor(Color newColor)
	{
		requiredLaserColor = newColor;
		GetComponentInChildren<Renderer>().material.color = requiredLaserColor;
		GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", Color.black);
	}

	public void CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut)
	{
		hitPointOut = hitPoint;
		hitNormalOut = hitNormal;
	}

	public void OnLaserHitted(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		Debug.Log("OnLaserHitted: Obsorber: " + hittedLaser.color.ToString());
		if (hittedLaser.color == requiredLaserColor)
		{
			laser = hittedLaser;
			SetActiveState(true);
			laser.OnObjectChanged += OnLaserChanged;
		}
	}

	private void OnLaserChanged()
	{
		laser.OnObjectChanged -= OnLaserChanged;
		laser = null;
		SetActiveState(false);
	}

	private void SetActiveState(bool _isActive)
	{
		if (isActive == _isActive)
			return;

		isActive = _isActive;
		if (isActive)
			GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", requiredLaserColor);
		else
			GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", Color.black);
		OnActiveStateChanged?.Invoke(isActive);
	}
}

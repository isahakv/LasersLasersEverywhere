using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserObsorber : Item, IObstacle
{
	public Color requiredLaserColor;
	public float activeStateChangeTime = 3f;
	public Action OnObjectChanged { get; set; }
	public Action<bool> OnActiveStateChanged;

	bool isActive = false;
	Laser laser;
	public float activeStateChangeCounter = 0f;
	Coroutine setActiveStateCoroutine;

	public override void SetColor(Color[] inputColors, Color[] outputColors)
	{
		requiredLaserColor = inputColors[0];
		GetComponentInChildren<Renderer>().material.color = requiredLaserColor;
		GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", Color.black);
	}

	public bool CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut)
	{
		hitPointOut = hitPoint;
		hitNormalOut = hitNormal;
		return true;
	}

	public void OnLaserHitted(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		Debug.Log("OnLaserHitted: Obsorber: " + hittedLaser.color.ToString());
		if (hittedLaser.color == requiredLaserColor)
		{
			laser = hittedLaser;
			laser.OnObjectChanged += OnLaserChanged;
			SetActiveState(true);
		}
	}

	private void OnLaserChanged()
	{
		laser.OnObjectChanged -= OnLaserChanged;
		laser = null;
		SetActiveState(false);
	}

	private IEnumerator SetActiveStateCoroutine(bool _isActive)
	{
		Color startColor = Color.black, endColor = requiredLaserColor;
		while ((_isActive && activeStateChangeCounter < activeStateChangeTime) || (!_isActive && activeStateChangeCounter > 0f))
		{
			float alpha = activeStateChangeCounter / activeStateChangeTime;
			Color color = Color.Lerp(startColor, endColor, alpha);
			GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", color);

			activeStateChangeCounter += _isActive ? Time.deltaTime : -Time.deltaTime;
			yield return null;
		}
		if (_isActive)
		{
			isActive = _isActive;
			OnActiveStateChanged?.Invoke(isActive);
		}
	}

	private void SetActiveState(bool _isActive)
	{
		if (!gameObject.activeInHierarchy)
			return;
		// If new active state if false, then change it immediately.
		if (isActive != _isActive && !_isActive)
		{
			isActive = _isActive;
			OnActiveStateChanged?.Invoke(isActive);
		}

		if (setActiveStateCoroutine != null)
			StopCoroutine(setActiveStateCoroutine);
		setActiveStateCoroutine = StartCoroutine(SetActiveStateCoroutine(_isActive));
	}
}

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

	Material material;
	Laser hittedLaser;
	bool isActive = false;
	float activeStateChangeCounter = 0f;
	Coroutine setActiveStateCoroutine;

	private void Awake()
	{
		material = GetComponentInChildren<Renderer>().material;
	}

	public override void SetColor(Color[] inputColors, Color[] outputColors)
	{
		requiredLaserColor = inputColors[0];
		material.color = requiredLaserColor;
		material.SetColor("_EmissionColor", Color.black);
	}

	public bool CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut)
	{
		hitPointOut = hitPoint;
		hitNormalOut = hitNormal;
		return true;
	}

	public void OnLaserHitted(Laser _hittedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		Debug.Log("OnLaserHitted: Obsorber: " + _hittedLaser.color.ToString());
		Color color = _hittedLaser.color;
		color.a = 1f;
		if (color == requiredLaserColor)
		{
			hittedLaser = _hittedLaser;
			hittedLaser.OnObjectChanged += OnLaserChanged;
			SetActiveState(true);
		}
	}

	private void OnLaserChanged()
	{
		hittedLaser.OnObjectChanged -= OnLaserChanged;
		hittedLaser = null;
		SetActiveState(false);
	}

	private IEnumerator SetActiveStateCoroutine(bool _isActive)
	{
		Color startColor = Color.black, endColor = requiredLaserColor;
		while ((_isActive && activeStateChangeCounter < activeStateChangeTime) || (!_isActive && activeStateChangeCounter > 0f))
		{
			float alpha = activeStateChangeCounter / activeStateChangeTime;
			Color color = Color.Lerp(startColor, endColor, alpha);
			material.SetColor("_EmissionColor", color);

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

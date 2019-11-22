using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBlockerDestructible : Item, IObstacle
{
	public Color requiredLaserColor;
	public Collider obstacleCollider;
	public GameObject explosionEffect;
	public Vector3 explosionEffectOffset;
	public float explosionForce = 10f;
	public float explosionTime = 5f;
	public float minExplosionEffectValue;
	public float maxExplosionEffectValue;
	public float piecesLifeSpan = 3.0f;
	public System.Action OnObjectChanged { get; set; }

	Material[] materials;
	Material explosionEffectMaterial;
	Laser hittedLaser;
	Coroutine explodingCoroutine;
	public float explosionCounter = 0f;

	private void Awake()
	{
		
	}

	public override void SetColor(Color[] inputColors, Color[] outputColors)
	{
		explosionEffectMaterial = explosionEffect.GetComponent<Renderer>().material;
		explosionEffectMaterial.SetVector("_WorldPosition", transform.position + explosionEffectOffset);

		if (materials == null)
		{
			Renderer[] renderers = GetComponentsInChildren<Renderer>();
			materials = new Material[renderers.Length];
			for (int i = 0; i < renderers.Length; i++)
				materials[i] = renderers[i].material;
		}

		requiredLaserColor = inputColors[0];
		foreach (Material mat in materials)
		{
			mat.color = requiredLaserColor;
			mat.SetColor("_EmissionColor", Color.black);
		}
	}

	public bool CalcLaserHitPos(Laser hittedLaser, Vector3 hitPoint, Vector3 hitNormal, out Vector3 hitPointOut, out Vector3 hitNormalOut)
	{
		hitPointOut = hitPoint;
		hitNormalOut = hitNormal;
		return true;
	}

	public void OnLaserHitted(Laser _hittedLaser, Vector3 hitPoint, Vector3 hitNormal)
	{
		Color color = _hittedLaser.color;
		color.a = 1f;
		if (color == requiredLaserColor)
		{
			hittedLaser = _hittedLaser;
			hittedLaser.OnObjectChanged += OnLaserChanged;
			// Start exploding coroutine.
			if (explodingCoroutine != null)
				StopCoroutine(explodingCoroutine);
			explodingCoroutine = StartCoroutine(ExplodingCoroutine(true));
		}
	}

	private void OnLaserChanged()
	{
		if (!gameObject.activeInHierarchy)
			return;

		hittedLaser.OnObjectChanged -= OnLaserChanged;
		hittedLaser = null;
		if (explodingCoroutine != null)
			StopCoroutine(explodingCoroutine);
		explodingCoroutine = StartCoroutine(ExplodingCoroutine(false));
	}

	IEnumerator ExplodingCoroutine(bool isExploding)
	{
		while((isExploding && explosionCounter < explosionTime)
				|| (!isExploding && explosionCounter > 0f))
		{
			float alpha = explosionCounter / explosionTime;
			float lerpedValue = Mathf.Lerp(minExplosionEffectValue, maxExplosionEffectValue, alpha);
			explosionEffectMaterial.SetFloat("_SphereRadius", lerpedValue);

			explosionCounter += isExploding ? Time.deltaTime : -Time.deltaTime;
			yield return null;
		}

		if (isExploding)
			OnExploded();
	}

	void OnExploded()
	{
		Destroy(obstacleCollider.gameObject);
		Destroy(explosionEffect);
		Rigidbody[] rgs = GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rg in rgs)
		{
			rg.isKinematic = false;
			rg.AddForceAtPosition(Random.insideUnitSphere * explosionForce, transform.position, ForceMode.Impulse);
		}
		// Notify laser that it changed.
		OnObjectChanged?.Invoke();

		StartCoroutine(FadeOutPieces());
	}

	IEnumerator FadeOutPieces()
	{
		float counter = piecesLifeSpan;
		Color color;
		while (counter > 0)
		{
			foreach (Material mat in materials)
			{
				color = mat.color;
				color.a = counter / piecesLifeSpan;
				mat.color = color;
			}

			counter -= Time.deltaTime;
			yield return null;
		}
		Destroy(gameObject);
	}
		
	private void OnDestroy()
	{
		OnObjectChanged?.Invoke();
		if (hittedLaser)
			hittedLaser.OnObjectChanged -= OnLaserChanged;
	}
}

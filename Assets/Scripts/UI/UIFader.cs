using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFader : MonoBehaviour
{
	public CanvasGroup elementToFade;
	public float fadeInTime = 0.1f, fadeOutTime = 1f;

	public void FadeIn()
	{
		StopAllCoroutines();
		StartCoroutine(FadeCoroutine(1f, fadeInTime));
	}

	public void FadeOut()
	{
		StopAllCoroutines();
		StartCoroutine(FadeCoroutine(0f, fadeOutTime));
	}

	public void Fade(float alpha, float fadeTime)
	{
		StopAllCoroutines();
		StartCoroutine(FadeCoroutine(alpha, fadeTime));
	}
	
	IEnumerator FadeCoroutine(float targetAlpha, float fadeTime)
	{
		float currFadeTime = 0f;
		float startAlpha = elementToFade.alpha;

		while (currFadeTime <= fadeTime)
		{
			elementToFade.alpha = Mathf.Lerp(startAlpha, targetAlpha, currFadeTime / fadeTime);

			currFadeTime += Time.deltaTime;
			yield return null;
		}
		elementToFade.alpha = targetAlpha;
	}
}

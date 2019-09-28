using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PercentageBarUI : MonoBehaviour
{
	public Text percentText;
	public UIFader fader;

	/// <summary>
	/// Sets percent text value.
	/// </summary>
	/// <param name="percentage">Percent value 0 to 1.</param>
	public void SetValue(float percentage)
	{
		percentage = Mathf.Clamp(percentage, 0f, 1f) * 100f;
		if (percentage > 99f)
			percentage = 100f;
		else if (percentage < 1f)
			percentage = 0f;
		percentText.text = Mathf.RoundToInt(percentage) + "%";
	}

	public void FadeIn()
	{
		fader?.FadeIn();
	}

	public void FadeOut()
	{
		fader?.FadeOut();
	}
}

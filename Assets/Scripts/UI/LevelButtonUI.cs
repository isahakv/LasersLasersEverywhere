using UnityEngine;
using UnityEngine.UI;

public class LevelButtonUI : MonoBehaviour
{
	public Text buttonText;
	private int levelIndex;
	public int LevelIndex
	{
		get { return levelIndex; }
		set
		{
			levelIndex = value;
			buttonText.text = (levelIndex + 1).ToString();
		}
	}
}

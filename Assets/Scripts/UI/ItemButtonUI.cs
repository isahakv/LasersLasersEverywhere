using UnityEngine;
using UnityEngine.UI;

public class ItemButtonUI : MonoBehaviour
{
	public Sprite buttonSpriteActive, buttonSpriteDeactive;
	public Text itemCount_Text;
	public ItemType itemType;
	bool isActive;

	public void SetActiveState(bool active)
	{
		isActive = active;
		GetComponent<Image>().sprite = isActive ? buttonSpriteActive : buttonSpriteDeactive;
	}

	public void SetItemCount(int count)
	{
		if (itemCount_Text != null)
			itemCount_Text.text = count.ToString();
	}
}

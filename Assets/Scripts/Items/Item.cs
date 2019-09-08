using UnityEngine;

public enum ItemType
{
	None,
	Reflector,
	LaserSplitter,
	LaserBeamer,
	LaserAbsorber
}

public class Item : MonoBehaviour, IInteractible
{
	public ItemType itemType;
	public bool isInteractible;

	public virtual void SetColor(Color[] inputColors, Color[] outputColors) { }

	public bool IsInteractible()
	{
		return isInteractible;
	}

	public virtual void RotateItem()
	{
		transform.Rotate(0f, 90f, 0f);
	}

	public virtual void RemoveItem()
	{
		Destroy(gameObject);
	}

	public virtual ItemType GetItemType()
	{
		return itemType;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
	None,
	Reflector,
	LaserBeamer,
	LaserAbsorber
}

public class Item : MonoBehaviour, IInteractible
{
	public ItemType itemType;
	public bool isInteractible;
	bool flip = false;

	public virtual void SetColor(Color newColor) { }

	public bool IsInteractible()
	{
		return isInteractible;
	}

	public virtual void RotateItem()
	{
		/*if (transform.rotation.eulerAngles.y <= -45)
			transform.rotation = Quaternion.Euler(0, 45, 0);
		else
			transform.rotation = Quaternion.Euler(0, -45, 0);*/

		if (flip)
			transform.Rotate(0f, 90f, 0f);
		else
			transform.Rotate(0f, -90f, 0f);

		flip = !flip;
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

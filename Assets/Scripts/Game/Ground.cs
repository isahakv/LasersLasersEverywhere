using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour, IPlaceable
{
	public Item placedItem;

	public bool HavePlacedItem()
	{
		return placedItem != null;
	}

	public Vector3 GetItemPlacePos()
	{
		return transform.position;
	}

	public void SetPlacedItem(Item item)
	{
		placedItem = item;
	}
}

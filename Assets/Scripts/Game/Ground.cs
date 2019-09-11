using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour, IPlaceable
{
	private Item placedItem;

	public bool HavePlacedItem()
	{
		return placedItem != null;
	}

	public Vector3 GetItemPlacePos()
	{
		return transform.position;
	}

	public Item GetPlacedItem()
	{
		return placedItem;
	}

	public void SetPlacedItem(Item item)
	{
		placedItem = item;
	}
}

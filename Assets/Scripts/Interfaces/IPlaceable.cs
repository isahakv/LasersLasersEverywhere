using UnityEngine;

public interface IPlaceable
{
	bool HavePlacedItem();
	Vector3 GetItemPlacePos();
	void SetPlacedItem(Item item);
}

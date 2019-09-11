using UnityEngine;

public interface IPlaceable
{
	bool HavePlacedItem();
	Vector3 GetItemPlacePos();
	Item GetPlacedItem();
	void SetPlacedItem(Item item);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
	public ItemType itemType;
	public int count;

	public InventoryItem(ItemType _itemType, int _count)
	{
		itemType = _itemType;
		count = _count;
	}
}

[System.Serializable]
public class Inventory
{
	[SerializeField]
	InventoryItem[] items;

	public Inventory(InventoryItem[] _items)
	{
		items = _items;
		// if (_items == null)
		// 	return;
		// foreach (InventoryItem item in _items)
		// 	items.Add(item);
	}

	public Inventory Copy()
	{
		InventoryItem[] _items = new InventoryItem[items.Length];
		for (int i = 0; i < items.Length; i++)
			_items[i] = new InventoryItem(items[i].itemType, items[i].count);

		Inventory inv = new Inventory(_items);
		return inv;
	}

	public InventoryItem GetItem(ItemType type)
	{
		foreach (InventoryItem item in items)
		{
			if (item.itemType == type)
				return item;
		}
		return null;
	}

	public InventoryItem[] GetItems()
	{
		return items;
	}
}

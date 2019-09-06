using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
	public ItemType type;
	public int count;

	public InventoryItem(ItemType _type, int _count)
	{
		type = _type;
		count = _count;
	}
}

[System.Serializable]
public class Inventory
{
	[SerializeField]
	List<InventoryItem> items;

	public Inventory(InventoryItem[] _items = null)
	{
		items = new List<InventoryItem>();
		if (_items == null)
			return;
		foreach (InventoryItem item in _items)
			items.Add(item);
	}

	public Inventory Copy()
	{
		Inventory inv = new Inventory();
		foreach (InventoryItem item in items)
			inv.items.Add(new InventoryItem(item.type, item.count));
		return inv;
	}

	public InventoryItem GetItem(ItemType type)
	{
		foreach (InventoryItem item in items)
		{
			if (item.type == type)
				return item;
		}
		return null;
	}

	public InventoryItem[] GetItems()
	{
		return items.ToArray();
	}
}

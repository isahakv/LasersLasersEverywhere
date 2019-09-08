using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlatformType
{
	None,
	Empty,
	Block,
}

[System.Serializable]
public struct ItemData
{
	public ItemType type;
	public Color[] inputColors, outputColors;
	public Vector3 direction;

	/*public ItemData()
	{
		type = ItemType.None;
		direction = Vector3.forward;
	}*/
}

[System.Serializable]
public class Platform
{
	public int row, column;
	public PlatformType type;
	public ItemData itemData;

	public Platform(int _row, int _column)
	{
		row = _row;
		column = _column;
		type = PlatformType.None;
		itemData = new ItemData();
	}
}

[CreateAssetMenu(fileName = "NewLevel", menuName = "Game Level")]
public class Level : ScriptableObject
{
	public bool isCompleted = false;
	public int gridWidth, gridHeight;
	public Platform[] map;
	public Inventory inventory;
}

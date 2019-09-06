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
public class Platform
{
	public int row, column;
	public PlatformType type;
	// public bool canPlaceItem;
	public ItemType itemType;
	public Color color;
	public Vector3 direction;

	public Platform(int _row, int _column)
	{
		row = _row;
		column = _column;
		type = PlatformType.None;
		color = Color.white;
		direction = Vector3.forward;
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

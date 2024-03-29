﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }
	public GameMap gameMap { get; private set; }
	public GameObject gameMapObject;
	[SerializeField]
	private Level[] levels = null;

	Inventory inventory;
	
	private void Awake()
	{
		if (Instance == null)
		{
			DontDestroyOnLoad(gameObject);
			Instance = this;
			gameMap = gameMapObject.GetComponent<GameMap>();
		}
		else if (Instance != this)
			DestroyImmediate(gameObject);
	}

	public void ResetGameProgress()
	{
		PlayerPrefs.SetInt("CurrentLvl", 0);
		// Lock all levels, except first.
		for (int i = 1; i < levels.Length; i++)
			levels[i].isLocked = true;
		PlayerPrefs.Save();
	}

	public int GetLevelsCount()
	{
		return levels.Length;
	}

	public int GetCurrentLevelIdx()
	{
		return PlayerPrefs.GetInt("CurrentLvl", 0);
	}

	public bool IsLevelLocked(int idx)
	{
		return levels[idx].isLocked;
	}

	public void LoadCurrentLevel()
	{
		int currLvl = GetCurrentLevelIdx();
		LoadLevel(currLvl);
	}

	public void LoadNextLevel()
	{
		int nextLvl = 1 + GetCurrentLevelIdx();
		if (nextLvl < levels.Length)
			LoadLevel(nextLvl);
	}

	public void UnloadCurrentLevel()
	{
		gameMap.DestroyMap();
	}

	public void LoadLevel(int idx)
	{
		Level level = levels[idx];
		gameMap.SpawnMap(level.map, level.gridWidth, level.gridHeight, LevelCompleted);
		// Setup Inventory.
		inventory = level.inventory.Copy();
		// Save this Level as current Level.
		PlayerPrefs.SetInt("CurrentLvl", idx);
		PlayerPrefs.Save();
		// Update HUD.
		UIManager.Instance.UpdateHUD(inventory);
	}

	private void LevelCompleted()
	{
		int nextLvl = 1 + GetCurrentLevelIdx();
		if (nextLvl < levels.Length)
			levels[nextLvl].isLocked = false;
		UIManager.Instance.SwitchMenu(MenuType.LevelCompleteMenu);
	}

	public void OnObjectInteracted(IInteractible interactible)
	{
		if (!interactible.IsInteractible())
			return;

		ItemType selectedItemType = UIManager.Instance.GetSelectedItemType();
		if (selectedItemType == ItemType.None)
		{
			ItemType removedItemType = interactible.GetItemType();
			interactible.RemoveItem();
			InventoryItem invItem = inventory.GetItem(removedItemType);
			invItem.count++;
			UIManager.Instance.UpdateHUD(inventory);
		}
		else
			interactible.RotateItem();
	}

	public void OnObjectInteracted(IPlaceable placeable)
	{
		// If on placeable already placed item, then call "OnObjectInteracted" for that item.
		if (placeable.HavePlacedItem())
		{
			IInteractible interactible = placeable.GetPlacedItem().GetComponent<IInteractible>();
			OnObjectInteracted(interactible);
			return;
		}

		ItemType selectedItemType = UIManager.Instance.GetSelectedItemType();
		if (selectedItemType == ItemType.None)
			return;

		InventoryItem invItem = inventory.GetItem(selectedItemType);
		if (invItem.count == 0)
			return;

		invItem.count--;
		Vector3 pos = placeable.GetItemPlacePos();
		Item item = gameMap.SpawnItem(selectedItemType, pos);
		item.isInteractible = true;
		placeable.SetPlacedItem(item);
		UIManager.Instance.UpdateHUD(inventory);
	}
}

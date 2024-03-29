﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class GameMap : MonoBehaviour
{
	[System.Serializable]
	public class PlatformPrefab
	{
		public PlatformType type;
		public GameObject prefab;
	}

	[System.Serializable]
	public class ItemPrefab
	{
		public ItemType type;
		public Item prefab;
	}

	public PlatformPrefab[] platformPrefabs;
	public ItemPrefab[] itemPrefabs;
	public Laser laserPrefab;
	public float platformSize = 1f;

	List<LaserBeamer> laserBeamers = new List<LaserBeamer>();
	List<LaserObsorber> laserObsorbers = new List<LaserObsorber>();
	int activeObsorberCount = 0;
	Action onAllObsorbersActivated;

	GameObject GetPlatformPrefab(PlatformType type)
	{
		foreach (PlatformPrefab platform in platformPrefabs)
		{
			if (platform.type == type)
				return platform.prefab;
		}
		return null;
	}

	Item GetItemPrefab(ItemType type)
	{
		foreach (ItemPrefab item in itemPrefabs)
		{
			if (item.type == type)
				return item.prefab;
		}
		return null;
	}

	public void SpawnMap(Platform[] map, int mapWidth, int mapHeight, Action _onAllObsorbersActivated)
	{
		DestroyMap();
		onAllObsorbersActivated = _onAllObsorbersActivated;

		foreach (Platform platform in map)
		{
			int columnIdx = platform.column - mapWidth / 2;
			int rowIdx = platform.row - mapHeight / 2;
			Vector3 pos = new Vector3(columnIdx * platformSize, 0f, -rowIdx * platformSize);
			GameObject platformGO = Instantiate(GetPlatformPrefab(platform.type), pos, Quaternion.identity, transform);
			// Place Item onto Platform.
			if (platform.itemData.type != ItemType.None)
			{
				IPlaceable placeable = platformGO.GetComponent<IPlaceable>();
				if (placeable == null)
					return;
				Item item = Instantiate(GetItemPrefab(platform.itemData.type), pos, Quaternion.identity, transform);
				item.transform.forward = platform.itemData.direction;
				item.SetColor(platform.itemData.inputColors, platform.itemData.outputColors);
				if (platform.itemData.type == ItemType.LaserBeamer)
					laserBeamers.Add((LaserBeamer)item);
				else if (platform.itemData.type == ItemType.LaserAbsorber)
				{
					((LaserObsorber)item).OnActiveStateChanged += OnObsorberActiveStateChanged;
					laserObsorbers.Add((LaserObsorber)item);
				}

				placeable.SetPlacedItem(item);
			}
		}
		// Spawn Lasers from LaserBeamers.
		foreach (LaserBeamer laserBeamer in laserBeamers)
		{
			Vector3 dir = laserBeamer.laserSpawnPos.forward;
			Vector3 pos = laserBeamer.laserSpawnPos.position;
			SpawnLaser(laserBeamer.laserColor, pos, dir, laserBeamer.GetComponent<IObstacle>(), null);
		}
	}

	public void DestroyMap()
	{
		laserBeamers = new List<LaserBeamer>();
		laserObsorbers = new List<LaserObsorber>();
		for (int i = transform.childCount - 1; i >= 0; i--)
			DestroyImmediate(transform.GetChild(i).gameObject);
	}

	public void SpawnLaser(Color color, Vector3 startPoint, Vector3 direction, IObstacle causerObstacle, Laser[] parents)
	{
		Laser laser = Instantiate(laserPrefab, startPoint, Quaternion.LookRotation(direction), transform);
		if (parents != null)
		{
			for (int i = 0; i < parents.Length; i++)
				parents[i].AddChild(laser);
		}
		laser.Init(color, startPoint, direction, causerObstacle, parents);
	}

	public Item SpawnItem(ItemType type, Vector3 pos)
	{
		return Instantiate(GetItemPrefab(type), pos, Quaternion.identity, transform);
	}

	private void OnObsorberActiveStateChanged(bool isActive) // TODO: Make this more SAFE.
	{
		activeObsorberCount += isActive ? 1 : -1;
		if (activeObsorberCount == laserObsorbers.Count)
			onAllObsorbersActivated();
	}
}

#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class LevelEditor : EditorWindow
{
	Level openedLevel = null;
	string openedLevelFilePath = "";
	Platform[,] grid;
	string[] gridTexts;
	int selectedGridIdx;
	Platform currentPlatform;
	public InventoryItem[] inventoryItems;

	private GUIStyle headerStyle = new GUIStyle();

	[MenuItem("Editor/Level Editor")]
	static public void Init()
	{
		LevelEditor window = GetWindow<LevelEditor>();
		window.titleContent = new GUIContent("Level Editor");
		window.minSize = new Vector2(1024, 720);
		window.Show();
	}

	private void OnEnable()
	{
		headerStyle.fontSize = 18;
	}

	private void OnGUI()
	{
		DrawMenu();
		AddSeparator();
		EditorGUILayout.BeginHorizontal();
		DrawLevelGrid();
		EditorGUILayout.BeginVertical();
		DrawPlatformProps();
		AddSeparator();
		DrawInventoryProps();
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
	}

	private void AddSeparator()
	{
		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
	}

	private void DrawMenu()
	{
		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Create Level"))
			CreateLevel();
		if (GUILayout.Button("Open Level"))
			OpenLevel();
		EditorGUI.BeginDisabledGroup(openedLevel == null);
		if (GUILayout.Button("Save Level"))
			SaveLevel();
		if (GUILayout.Button("Save As New Level"))
			SaveAsNewLevel();
		EditorGUI.EndDisabledGroup();
		
		EditorGUILayout.EndHorizontal();
	}

	private void DrawLevelGrid()
	{
		if (openedLevel == null)
			return;

		EditorGUILayout.BeginVertical();
		EditorGUILayout.BeginHorizontal();

		EditorGUIUtility.labelWidth = 80f;
		int gridWidth = EditorGUILayout.IntField("Grid Width", openedLevel.gridWidth, GUILayout.Width(120f));
		gridWidth = Mathf.Max(5, gridWidth);
		int gridHeight = EditorGUILayout.IntField("Grid Height", openedLevel.gridHeight, GUILayout.Width(120f));
		gridHeight = Mathf.Max(5, gridHeight);
		if (openedLevel.gridWidth != gridWidth || openedLevel.gridHeight != gridHeight)
		{
			openedLevel.gridWidth = gridWidth;
			openedLevel.gridHeight = gridHeight;
			ResizeGrid(gridWidth, gridHeight);
		}

		EditorGUILayout.EndHorizontal();

		float width = openedLevel.gridWidth * 65f, height = openedLevel.gridHeight *  65f;
		int newSelectedGridIdx = GUILayout.SelectionGrid(selectedGridIdx, gridTexts, openedLevel.gridWidth, GUILayout.Width(width), GUILayout.Height(height));
		if (newSelectedGridIdx != selectedGridIdx)
		{
			selectedGridIdx = newSelectedGridIdx;
			currentPlatform = GetPlatformAtIdx(newSelectedGridIdx);
		}

		EditorGUILayout.EndVertical();
	}

	private void DrawPlatformProps()
	{
		if (openedLevel == null || currentPlatform == null)
			return;

		EditorGUILayout.BeginVertical();
		EditorGUILayout.LabelField("Platform Properties", headerStyle);
		// Handle Platform Part.
		EditorGUIUtility.labelWidth = 100f;
		PlatformType platformType = (PlatformType)EditorGUILayout.EnumPopup("Platform Type", currentPlatform.type);
		ItemType itemType = currentPlatform.itemType;
		if (currentPlatform.type == PlatformType.Empty)
		{
			// Handle Platfrom Item Part.
			itemType = (ItemType)EditorGUILayout.EnumPopup("Item Type", currentPlatform.itemType);
			if (itemType != ItemType.None)
			{
				currentPlatform.color = EditorGUILayout.ColorField("Item Color ", currentPlatform.color);
				currentPlatform.direction = EditorGUILayout.Vector3Field("Item Direction ", currentPlatform.direction);
			}
		}

		if (platformType != currentPlatform.type || currentPlatform.itemType != itemType)
		{
			currentPlatform.type = platformType;
			currentPlatform.itemType = itemType;
			UpdateGridText(currentPlatform.row, currentPlatform.column);
		}

		EditorGUILayout.EndVertical();
	}

	private void DrawInventoryProps()
	{
		if (openedLevel == null)
			return;

		EditorGUILayout.BeginVertical();
		EditorGUILayout.LabelField("Inventory Properties", headerStyle);

		SerializedObject so = new SerializedObject(this);
		SerializedProperty itemsProperty = so.FindProperty("inventoryItems");
		EditorGUILayout.PropertyField(itemsProperty, true);
		so.ApplyModifiedProperties();

		EditorGUILayout.EndVertical();
	}

	private void InitGrid(Platform[] map = null)
	{
		int height = openedLevel.gridHeight;
		int width = openedLevel.gridWidth;
		grid = new Platform[height, width];
		// Initialize grid elements.
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
				grid[i, j] = new Platform(i, j);
		}
		// Fill grid elements with map values.
		if (map != null)
		{
			foreach (Platform platform in map)
				grid[platform.row, platform.column] = platform;
		}
		UpdateGridTexts();
	}

	private void ResizeGrid(int newWidth, int newHeight)
	{
		int oldHeight = grid.GetLength(0);
		int oldWidth = grid.GetLength(1);
		Platform[,] newGrid = new Platform[newHeight, newWidth];

		int minHeight = Mathf.Min(oldHeight, newHeight);
		int minWidth = Mathf.Min(oldWidth, newWidth);
		for (int i = 0; i < minHeight; i++)
		{
			for (int j = 0; j < minWidth; j++)
				newGrid[i, j] = grid[i, j]; // Filling new grid from old grid.
		}

		// Initialize extended part.
		if (newHeight > oldHeight)
		{
			for (int i = oldHeight; i < newHeight; i++)
			{
				for (int j = 0; j < minWidth; j++)
					newGrid[i, j] = new Platform(i, j);
			}
		}
		if (newWidth > oldWidth)
		{
			for (int i = 0; i < newHeight; i++)
			{
				for (int j = oldWidth; j < newWidth; j++)
					newGrid[i, j] = new Platform(i, j);
			}
		}
		// Assing new created grid to "grid".
		grid = newGrid;
		UpdateGridTexts();
	}

	private void UpdateGridTexts()
	{
		int height = grid.GetLength(0);
		int width = grid.GetLength(1);
		gridTexts = new string[height * width];
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
				UpdateGridText(i, j);
		}
	}

	private void UpdateGridText(int row, int column)
	{
		Platform platform = grid[row, column];
		string text;
		if (platform.type == PlatformType.None)
			text = "";
		else if (platform.type == PlatformType.Empty && platform.itemType != ItemType.None)
			text = platform.itemType.ToString();
		else
			text = platform.type.ToString();

		// Adding new line after UpperCases.
		for (int i = text.Length - 1; i > 0; i--)
		{
			char c = text[i];
			if (char.IsUpper(c))
				text = text.Insert(i, "\n");
		}

		int width = grid.GetLength(1);
		gridTexts[(row * width) + column] = text;
	}

	private Platform GetPlatformAtIdx(int index)
	{
		int row = index / openedLevel.gridWidth;
		int column = index % openedLevel.gridWidth;
		return grid[row, column];
	}

	private Platform[] GetMapFromGrid(Platform[,] grid)
	{
		List<Platform> map = new List<Platform>();
		for (int i = 0; i < grid.GetLength(0); i++)
		{
			for (int j = 0; j < grid.GetLength(1); j++)
			{
				if (grid[i, j].type != PlatformType.None)
					map.Add(grid[i, j]);
			}
		}
		return map.ToArray();
	}

	private void CreateLevel()
	{
		openedLevelFilePath = "";
		UpdateWindowTitle();
		openedLevel = CreateInstance<Level>();
		inventoryItems = null;
		InitGrid();
	}

	private void OpenLevel()
	{
		string absoluteLevelPath = EditorUtility.OpenFilePanel("Select Level...", Application.dataPath, "asset");
		if (!(absoluteLevelPath.StartsWith(Application.dataPath)))
			EditorUtility.DisplayDialog("ERROR", "Select File from Project Folder!", "Ok");
		else
		{
			string relativeLevelPath = GetRelativePath(absoluteLevelPath);
			openedLevel = AssetDatabase.LoadAssetAtPath<Level>(relativeLevelPath);
			if (openedLevel != null)
			{
				InitGrid(openedLevel.map);
				inventoryItems = openedLevel.inventory.GetItems();
				openedLevelFilePath = relativeLevelPath;
				UpdateWindowTitle();
			}
			else
				EditorUtility.DisplayDialog("ERROR", "Can't open selected file!", "Ok");
		}
	}

	private void SaveLevel()
	{
		if (openedLevel == null)
			return;

		// Convert Grid to Map.
		openedLevel.map = GetMapFromGrid(grid);
		openedLevel.inventory = new Inventory(inventoryItems);

		// Check if first time save.
		if (string.IsNullOrEmpty(openedLevelFilePath))
		{
			openedLevelFilePath = SaveAsNewLevel();
			UpdateWindowTitle();
			return;
		}
		EditorUtility.SetDirty(openedLevel);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	/// <returns>Returns path of the saved level.</returns>
	private string SaveAsNewLevel()
	{
		if (openedLevel == null)
			return "";

		string savePath = EditorUtility.SaveFilePanelInProject("Save Level...", "", "asset", "");
		if (string.IsNullOrEmpty(openedLevelFilePath))
		{
			openedLevelFilePath = savePath;
			AssetDatabase.CreateAsset(openedLevel, savePath);
		}
		else
			AssetDatabase.CopyAsset(openedLevelFilePath, savePath);
			
		EditorUtility.SetDirty(openedLevel);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		return savePath;
	}

	private string GetRelativePath(string absolutePath)
	{
		if (absolutePath == "")
			return "";

		return "Assets" + absolutePath.Substring(Application.dataPath.Length);
	}

	private void UpdateWindowTitle()
	{
		string levelName;
		if (string.IsNullOrEmpty(openedLevelFilePath))
			levelName = "Unsaved Level";
		else
			levelName = Path.GetFileName(openedLevelFilePath);

		LevelEditor window = GetWindow<LevelEditor>();
		window.titleContent = new GUIContent("Level Editor - " + levelName);
	}
}
#endif

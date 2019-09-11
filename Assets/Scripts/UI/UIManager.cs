using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public enum MenuType
	{
		HUD,
		MainMenu,
		SettingsMenu,
		InGameMenu,
		LevelSelectMenu,
		LevelCompleteMenu
	}

	public class UIManager : MonoBehaviour
	{
		public static UIManager Instance { get; private set; }
		public BaseMenu hud, mainMenu, settingsMenu, inGameMenu, levelSelectMenu, levelCompeteMenu;

		BaseMenu currentMenu, previousMenu;

		private void Awake()
		{
			if (Instance == null)
			{
				DontDestroyOnLoad(this);
				Instance = this;
			}
			else if (Instance != this)
				DestroyImmediate(this);
		}

		private void Start()
		{
			SwitchMenu(mainMenu);
		}

		public void SwitchMenu(MenuType menuType)
		{
			BaseMenu menuToSwitch = null;
			switch(menuType)
			{
				case MenuType.HUD: menuToSwitch = hud; break;
				case MenuType.MainMenu: menuToSwitch = mainMenu; break;
				case MenuType.SettingsMenu: menuToSwitch = settingsMenu; break;
				case MenuType.InGameMenu: menuToSwitch = inGameMenu; break;
				case MenuType.LevelSelectMenu: menuToSwitch = levelSelectMenu; break;
				case MenuType.LevelCompleteMenu: menuToSwitch = levelCompeteMenu; break;
			}
			if (menuToSwitch)
				SwitchMenu(menuToSwitch);
		}

		private void SwitchMenu(BaseMenu menu)
		{
			if (!menu)
				return;

			currentMenu?.CloseMenu();
			menu.OpenMenu();
			previousMenu = currentMenu;
			currentMenu = menu;
		}

		public void SwitchToPreviousMenu()
		{
			BaseMenu tmp = currentMenu;
			currentMenu = previousMenu;
			previousMenu = tmp;

			previousMenu?.CloseMenu();
			currentMenu?.OpenMenu();
		}

		public void UpdateHUD(Inventory inventory)
		{
			((HUD)hud).SetInventory(inventory);
		}

		public ItemType GetSelectedItemType()
		{
			return ((HUD)hud).GetSelectedItemType();
		}
	}
}

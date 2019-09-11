using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class LevelSelectMenu : BaseMenu
	{
		public GridLayoutGroup gridLayout;
		public LevelButtonUI level_Button;
		public int gridWidth, gridHeight;

		private List<LevelButtonUI> level_Buttons;

		public override void OpenMenu()
		{
			base.OpenMenu();
			level_Buttons = new List<LevelButtonUI>();
			int levelCount = GameManager.Instance.GetLevelsCount();
			for (int i = 0; i < levelCount; i++)
			{
				bool isActiveLvl = !(GameManager.Instance.IsLevelLocked(i));
				SpawnLevelButton(i, isActiveLvl);
			}
		}

		public override void CloseMenu()
		{
			base.CloseMenu();
			for (int i = 0; i < level_Buttons.Count; i++)
				Destroy(level_Buttons[i].gameObject);
		}

		private void SpawnLevelButton(int index, bool isActiveLvl)
		{
			LevelButtonUI lvlButton = Instantiate(level_Button, gridLayout.transform);
			lvlButton.GetComponent<Button>().interactable = isActiveLvl;
			lvlButton.GetComponent<Button>().onClick.AddListener(() => LevelButtonPressed(index));
			lvlButton.LevelIndex = index;
			level_Buttons.Add(lvlButton);
		}

		private void LevelButtonPressed(int index)
		{
			UIManager.Instance.SwitchMenu(MenuType.HUD);
			InputController.Instance.EnableInput();
			GameManager.Instance.LoadLevel(index);
		}
	}
}

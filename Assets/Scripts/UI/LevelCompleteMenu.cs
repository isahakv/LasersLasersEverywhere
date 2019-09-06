using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class LevelCompleteMenu : BaseMenu
	{
		public Button nextLevelButton, mainMenuButton;

		protected override void Awake()
		{
			base.Awake();
			nextLevelButton.onClick.AddListener(NextLevelButtonPressed);
			mainMenuButton.onClick.AddListener(MainMenuButtonPressed);
		}

		private void NextLevelButtonPressed()
		{
			GameManager.Instance.LoadNextLevel();
			UIManager.Instance.SwitchMenu(MenuType.HUD);
		}

		private void MainMenuButtonPressed()
		{
			GameManager.Instance.UnloadCurrentLevel();
			UIManager.Instance.SwitchMenu(MenuType.MainMenu);
		}
	}
}

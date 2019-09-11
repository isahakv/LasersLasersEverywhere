using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class InGameMenu : BaseMenu
	{
		public Button continueButton, levelsButton, mainMenuButton;

		protected override void Awake()
		{
			base.Awake();
			continueButton.onClick.AddListener(ContinueButtonPressed);
			levelsButton.onClick.AddListener(LevelsButtonPressed);
			mainMenuButton.onClick.AddListener(MainMenuButtonPressed);
		}

		private void ContinueButtonPressed()
		{
			UIManager.Instance.SwitchMenu(MenuType.HUD);
			InputController.EnableInput();
			// GameManager.Instance.ResumeGame();
		}

		private void LevelsButtonPressed()
		{
			UIManager.Instance.SwitchMenu(MenuType.LevelSelectMenu);
		}

		private void MainMenuButtonPressed()
		{
			UIManager.Instance.SwitchMenu(MenuType.MainMenu);
			GameManager.Instance.UnloadCurrentLevel();
		}
	}
}

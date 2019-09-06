using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class MainMenu : BaseMenu
	{
		public Button playButton, settingsButton, levelsButton, exitButton;

		protected override void Awake()
		{
			base.Awake();
			playButton.onClick.AddListener(PlayButtonPressed);
			settingsButton.onClick.AddListener(SettingsButtonPressed);
			levelsButton.onClick.AddListener(LevelsButtonPressed);
			exitButton.onClick.AddListener(ExitButtonPressed);
		}

		private void PlayButtonPressed()
		{
			UIManager.Instance.SwitchMenu(MenuType.HUD);
			GameManager.Instance.LoadCurrentLevel();
		}

		private void SettingsButtonPressed()
		{
			UIManager.Instance.SwitchMenu(MenuType.SettingsMenu);
		}

		private void LevelsButtonPressed()
		{
			UIManager.Instance.SwitchMenu(MenuType.LevelSelectMenu);
		}

		private void ExitButtonPressed()
		{
			Application.Quit();
		}
	}
}

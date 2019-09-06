using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class SettingsMenu : BaseMenu
	{
		public Button resetProgressButton;

		protected override void Awake()
		{
			base.Awake();
			resetProgressButton.onClick.AddListener(ResetProgressButtonPressed);
		}

		private void ResetProgressButtonPressed()
		{
			// TODO: Show Popup.
			GameManager.Instance.ResetGameProgress();
		}
	}
}

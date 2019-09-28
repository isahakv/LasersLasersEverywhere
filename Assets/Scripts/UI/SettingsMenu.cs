using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class SettingsMenu : BaseMenu
	{
		public Slider cameraSpeedSlider;
		public Button resetProgressButton;

		protected override void Awake()
		{
			base.Awake();
			cameraSpeedSlider.onValueChanged.AddListener(CameraSpeedSliderValueChanged);
			resetProgressButton.onClick.AddListener(ResetProgressButtonPressed);
		}

		private void CameraSpeedSliderValueChanged(float value)
		{
			InputController.Instance.cameraRotationSpeed = value;
		}

		private void ResetProgressButtonPressed()
		{
			// TODO: Show Popup.
			GameManager.Instance.ResetGameProgress();
		}
	}
}

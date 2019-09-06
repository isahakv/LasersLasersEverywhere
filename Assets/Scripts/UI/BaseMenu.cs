using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class BaseMenu : MonoBehaviour
	{
		public GameObject content;
		public Button backButton;

		protected virtual void Awake()
		{
			backButton?.onClick.AddListener(BackButtonPressed);
		}

		public virtual void OpenMenu()
		{
			content.SetActive(true);
		}

		public virtual void CloseMenu()
		{
			content.SetActive(false);
		}

		protected virtual void BackButtonPressed()
		{
			UIManager.Instance.SwitchToPreviousMenu();
		}
	}
}

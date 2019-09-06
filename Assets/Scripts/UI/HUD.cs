using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class HUD : BaseMenu
	{
		public Button restart_Button, pause_Button;
		public GameObject inventoryPanel;
		public ItemButtonUI[] invItemButtonPrefabs;
		List<ItemButtonUI> invItemButtons = new List<ItemButtonUI>();
		int selectedInvItemButtonIdx;

		protected override void Awake()
		{
			base.Awake();
			restart_Button.onClick.AddListener(RestartButtonPressed);
			pause_Button.onClick.AddListener(PauseButtonPressed);
		}

		public void SetInventory(Inventory inventory)
		{
			// Removing old buttons.
			foreach (ItemButtonUI invItemButton in invItemButtons)
				Destroy(invItemButton.gameObject);
			invItemButtons.Clear();

			InventoryItem[] items = inventory.GetItems();
			for (int i = 0; i < items.Length; i++)
			{
				ItemButtonUI itemBtn = GetInvItemButtonPrefab(items[i].type);
				ItemButtonUI itemBtnInstance = Instantiate(itemBtn, inventoryPanel.transform);
				itemBtnInstance.SetItemCount(items[i].count);
				invItemButtons.Add(itemBtnInstance);
			}
			// Setup Remove Item Button.
			ItemButtonUI removeItemBtn = GetInvItemButtonPrefab(ItemType.None);
			ItemButtonUI removeItemBtnInstance = Instantiate(removeItemBtn, inventoryPanel.transform);
			invItemButtons.Add(removeItemBtnInstance);
			// Setup Button Press Listeners.
			for (int i = 0; i < invItemButtons.Count; i++)
			{
				int index = i;
				invItemButtons[i].GetComponent<Button>().onClick.AddListener(() => InvItemButtonPressed(index));
			}
			// Set default selected button to active.
			invItemButtons[selectedInvItemButtonIdx].SetActiveState(true);
		}

		public ItemType GetSelectedItemType()
		{
			return invItemButtons[selectedInvItemButtonIdx].itemType;
		}

		private ItemButtonUI GetInvItemButtonPrefab(ItemType type)
		{
			foreach (ItemButtonUI itemBtn in invItemButtonPrefabs)
			{
				if (itemBtn.itemType == type)
					return itemBtn;
			}
			return null;
		}

		private void RestartButtonPressed()
		{
			GameManager.Instance.LoadCurrentLevel();
		}

		private void PauseButtonPressed()
		{
			UIManager.Instance.SwitchMenu(MenuType.InGameMenu);
			InputController.DisableInput();
			// GameManager.Instance.PauseGame();
		}

		private void InvItemButtonPressed(int index)
		{
			invItemButtons[selectedInvItemButtonIdx].SetActiveState(false);
			Debug.Log(index);
			invItemButtons[index].SetActiveState(true);
			selectedInvItemButtonIdx = index;
		}
	}
}

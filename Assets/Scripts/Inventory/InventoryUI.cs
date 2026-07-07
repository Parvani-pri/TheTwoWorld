using TMPro;
using TwoWorlds.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace TwoWorlds.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] InputReader inputReader;
        [SerializeField] PlayerInventory playerInventory;
        [SerializeField] GameObject panelRoot;
        [SerializeField] Transform slotContainer;
        [SerializeField] InventorySlotUI slotPrefab;
        [SerializeField] TMP_Text itemNameText;
        [SerializeField] TMP_Text itemDescriptionText;
        [SerializeField] ItemDropper itemDropper;
        [SerializeField] UnityEngine.UI.Button discardButton;

        InventorySlotUI[] slotViews;
        bool isOpen;
        bool gameplayBlocked;
        int selectedSlotIndex = -1;

        void OnEnable()
        {
            GameEvents.InventoryChanged += OnInventoryChanged;
            GameEvents.GameplayInputBlocked += OnGameplayInputBlocked;
        }

        void OnDisable()
        {
            GameEvents.InventoryChanged -= OnInventoryChanged;
            GameEvents.GameplayInputBlocked -= OnGameplayInputBlocked;
        }

        void Start()
        {
            if (inputReader == null)
                inputReader = FindFirstObjectByType<InputReader>();

            if (inputReader?.InventoryAction == null)
                Debug.LogError("[InventoryUI] InputReader or Inventory action is missing.");

            if (playerInventory == null)
                playerInventory = FindFirstObjectByType<PlayerInventory>();

            if (itemDropper == null)
                itemDropper = FindFirstObjectByType<ItemDropper>();

            if (discardButton != null)
                discardButton.onClick.AddListener(DiscardSelectedItem);

            BuildSlotViews();
            SetOpen(false);
            Refresh();
        }

        void Update()
        {
            if (inputReader?.InventoryAction == null || (gameplayBlocked && !isOpen))
                return;

            if (!isOpen)
            {
                if (inputReader.InventoryAction.WasPerformedThisFrame())
                    SetOpen(true);
                return;
            }

            if (WantsCloseInventory())
                SetOpen(false);

            if (isOpen)
                HandleDiscardInput();
        }

        void HandleDiscardInput()
        {
            if (selectedSlotIndex < 0)
                return;

            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.deleteKey.wasPressedThisFrame || keyboard.xKey.wasPressedThisFrame)
                DiscardSelectedItem();
        }

        bool WantsCloseInventory()
        {
            if (inputReader.InventoryAction.WasPerformedThisFrame())
                return true;

            if (inputReader.CancelAction != null && inputReader.CancelAction.WasPerformedThisFrame())
                return true;

            var keyboard = Keyboard.current;
            if (keyboard == null)
                return false;

            return keyboard.tabKey.wasPressedThisFrame ||
                   keyboard.iKey.wasPressedThisFrame ||
                   keyboard.escapeKey.wasPressedThisFrame;
        }

        void BuildSlotViews()
        {
            if (slotContainer == null || slotPrefab == null || playerInventory == null)
                return;

            foreach (Transform child in slotContainer)
                Destroy(child.gameObject);

            slotViews = new InventorySlotUI[playerInventory.Capacity];
            for (var i = 0; i < playerInventory.Capacity; i++)
            {
                var slotView = Instantiate(slotPrefab, slotContainer);
                slotViews[i] = slotView;
            }
        }

        public void Toggle()
        {
            if (gameplayBlocked && !isOpen)
                return;

            SetOpen(!isOpen);
        }

        void SetOpen(bool open)
        {
            isOpen = open;
            if (panelRoot != null)
                panelRoot.SetActive(open);

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            if (!open)
            {
                selectedSlotIndex = -1;
                ClearItemDetails();
                RefreshDiscardButton();
            }
            else
            {
                Refresh();
            }
        }

        void OnInventoryChanged(PlayerInventory _) => Refresh();

        void OnGameplayInputBlocked(bool blocked)
        {
            gameplayBlocked = blocked;
            if (blocked && isOpen)
                SetOpen(false);
        }

        void Refresh()
        {
            if (playerInventory == null || slotViews == null)
                return;

            var slots = playerInventory.Slots;
            for (var i = 0; i < slotViews.Length; i++)
            {
                var slotIndex = i;
                var slot = i < slots.Count ? slots[i] : default;
                slotViews[i].Bind(slot, selected => OnSlotSelected(slotIndex, selected));
            }
        }

        void OnSlotSelected(int slotIndex, InventorySlot slot)
        {
            selectedSlotIndex = slot.IsEmpty ? -1 : slotIndex;
            ShowItemDetails(slot);
            RefreshDiscardButton();
        }

        void DiscardSelectedItem()
        {
            if (selectedSlotIndex < 0 || itemDropper == null || playerInventory == null)
                return;

            var slot = playerInventory.GetSlot(selectedSlotIndex);
            if (slot.IsEmpty)
            {
                selectedSlotIndex = -1;
                ClearItemDetails();
                RefreshDiscardButton();
                return;
            }

            if (!itemDropper.DropFromSlot(playerInventory, selectedSlotIndex, 1))
                return;

            var remaining = playerInventory.GetSlot(selectedSlotIndex);
            if (remaining.IsEmpty)
            {
                selectedSlotIndex = -1;
                ClearItemDetails();
            }
            else
            {
                ShowItemDetails(remaining);
            }

            RefreshDiscardButton();
        }

        void RefreshDiscardButton()
        {
            if (discardButton == null)
                return;

            var hasSelection = selectedSlotIndex >= 0 &&
                               !playerInventory.GetSlot(selectedSlotIndex).IsEmpty;
            discardButton.interactable = hasSelection;
        }

        void ClearItemDetails()
        {
            if (itemNameText != null)
                itemNameText.text = string.Empty;

            if (itemDescriptionText != null)
                itemDescriptionText.text = string.Empty;
        }

        void ShowItemDetails(InventorySlot slot)
        {
            if (itemNameText != null)
                itemNameText.text = slot.IsEmpty ? string.Empty : slot.item.DisplayName;

            if (itemDescriptionText != null)
                itemDescriptionText.text = slot.IsEmpty ? string.Empty : slot.item.Description;
        }

        void OnDestroy()
        {
            if (discardButton != null)
                discardButton.onClick.RemoveListener(DiscardSelectedItem);
        }
    }
}

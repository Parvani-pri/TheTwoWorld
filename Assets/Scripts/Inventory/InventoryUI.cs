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

        InventorySlotUI[] slotViews;
        bool isOpen;
        bool gameplayBlocked;

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

            if (open)
                Refresh();
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
                var slot = i < slots.Count ? slots[i] : default;
                slotViews[i].Bind(slot, ShowItemDetails);
            }
        }

        void ShowItemDetails(InventorySlot slot)
        {
            if (itemNameText != null)
                itemNameText.text = slot.IsEmpty ? string.Empty : slot.item.DisplayName;

            if (itemDescriptionText != null)
                itemDescriptionText.text = slot.IsEmpty ? string.Empty : slot.item.Description;
        }
    }
}

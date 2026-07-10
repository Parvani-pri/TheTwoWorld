using TMPro;
using TwoWorlds.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        [SerializeField] TMP_Text itemMetaText;
        [SerializeField] ItemDropper itemDropper;
        [SerializeField] Button discardButton;
        [SerializeField] Button discardAllButton;
        [SerializeField] Canvas dragCanvas;
        [SerializeField] float dragGhostSize = 64f;

        InventorySlotUI[] slotViews;
        bool isOpen;
        bool externalGameplayBlocked;
        int selectedSlotIndex = -1;
        int dragSourceIndex = -1;
        int framesSinceReady;
        RectTransform dragGhost;

        const int StartupInputIgnoreFrames = 3;

        void Awake()
        {
            isOpen = false;
            if (panelRoot != null)
                panelRoot.SetActive(false);

            ClearItemDetails();
        }

        void OnEnable()
        {
            GameEvents.InventoryChanged += OnInventoryChanged;
            GameEvents.GameplayInputBlocked += OnGameplayInputBlocked;
        }

        void OnDisable()
        {
            GameEvents.InventoryChanged -= OnInventoryChanged;
            GameEvents.GameplayInputBlocked -= OnGameplayInputBlocked;

            if (isOpen)
                SetOpen(false);
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

            if (dragCanvas == null)
                dragCanvas = GetComponentInParent<Canvas>();

            if (discardButton != null)
                discardButton.onClick.AddListener(() => DiscardSelectedItem(1));

            if (discardAllButton != null)
                discardAllButton.onClick.AddListener(() => DiscardSelectedItem(-1));

            BuildSlotViews();
            framesSinceReady = 0;
            SetOpen(false);
            Refresh();
        }

        void Update()
        {
            framesSinceReady++;

            if (inputReader?.InventoryAction == null || (externalGameplayBlocked && !isOpen))
                return;

            if (!isOpen)
            {
                if (framesSinceReady > StartupInputIgnoreFrames &&
                    inputReader.InventoryAction.WasReleasedThisFrame())
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

            var dropAll = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            var dropHalf = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;

            if (keyboard.deleteKey.wasPressedThisFrame || keyboard.xKey.wasPressedThisFrame)
            {
                if (dropAll)
                    DiscardSelectedItem(-1);
                else if (dropHalf)
                    DiscardSelectedItem(GetHalfDiscardAmount());
                else
                    DiscardSelectedItem(1);
            }
        }

        int GetHalfDiscardAmount()
        {
            if (selectedSlotIndex < 0 || playerInventory == null)
                return 1;

            var slot = playerInventory.GetSlot(selectedSlotIndex);
            if (slot.IsEmpty)
                return 1;

            return Mathf.Max(1, slot.quantity / 2);
        }

        bool WantsCloseInventory()
        {
            if (inputReader.InventoryAction.WasReleasedThisFrame())
                return true;

            if (inputReader.CancelAction != null && inputReader.CancelAction.WasReleasedThisFrame())
                return true;

            var keyboard = Keyboard.current;
            if (keyboard == null)
                return false;

            return keyboard.escapeKey.wasReleasedThisFrame;
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
            if (externalGameplayBlocked && !isOpen)
                return;

            SetOpen(!isOpen);
        }

        void SetOpen(bool open)
        {
            if (isOpen == open)
                return;

            isOpen = open;
            if (panelRoot != null)
                panelRoot.SetActive(open);

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            GameEvents.RaiseInventoryOpenChanged(open);

            if (!open)
            {
                selectedSlotIndex = -1;
                ClearDragGhost();
                ClearItemDetails();
                RefreshDiscardButtons();
            }
            else
            {
                selectedSlotIndex = -1;
                ClearItemDetails();
                RefreshDiscardButtons();
                Refresh();
            }
        }

        void OnInventoryChanged(PlayerInventory _) => Refresh();

        void OnGameplayInputBlocked(bool blocked)
        {
            externalGameplayBlocked = blocked;
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
                slotViews[i].Bind(
                    slotIndex,
                    slot,
                    i == selectedSlotIndex,
                    OnSlotSelected,
                    OnMoveRequested,
                    OnBeginSlotDrag,
                    OnSlotDrag,
                    OnEndSlotDrag);
            }
        }

        void OnSlotSelected(int slotIndex, InventorySlot slot)
        {
            selectedSlotIndex = slot.IsEmpty ? -1 : slotIndex;
            ShowItemDetails(slot);
            RefreshDiscardButtons();
            Refresh();
        }

        bool OnMoveRequested(int fromIndex, int toIndex)
        {
            if (playerInventory == null)
                return false;

            if (!playerInventory.MoveSlot(fromIndex, toIndex))
                return false;

            if (selectedSlotIndex == fromIndex)
                selectedSlotIndex = toIndex;
            else if (selectedSlotIndex == toIndex)
                selectedSlotIndex = fromIndex;

            ShowItemDetails(playerInventory.GetSlot(selectedSlotIndex));
            RefreshDiscardButtons();
            Refresh();
            return true;
        }

        void OnBeginSlotDrag(int sourceIndex, PointerEventData eventData)
        {
            if (playerInventory == null)
                return;

            dragSourceIndex = sourceIndex;
            var slot = playerInventory.GetSlot(sourceIndex);
            if (slot.IsEmpty)
                return;

            ClearDragGhost();

            if (dragCanvas == null || slot.item.Icon == null)
                return;

            var ghostObject = new GameObject("InventoryDragGhost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ghostObject.transform.SetParent(dragCanvas.transform, false);

            dragGhost = ghostObject.GetComponent<RectTransform>();
            dragGhost.sizeDelta = new Vector2(dragGhostSize, dragGhostSize);

            var ghostImage = ghostObject.GetComponent<Image>();
            ghostImage.sprite = slot.item.Icon;
            ghostImage.raycastTarget = false;
            ghostImage.preserveAspect = true;
            ghostImage.color = new Color(1f, 1f, 1f, 0.75f);

            OnSlotDrag(eventData);
        }

        void OnSlotDrag(PointerEventData eventData)
        {
            if (dragGhost == null)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    dragCanvas.transform as RectTransform,
                    eventData.position,
                    dragCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : dragCanvas.worldCamera,
                    out var localPoint))
            {
                dragGhost.localPosition = localPoint;
            }
        }

        void OnEndSlotDrag()
        {
            dragSourceIndex = -1;
            ClearDragGhost();
        }

        void ClearDragGhost()
        {
            if (dragGhost != null)
                Destroy(dragGhost.gameObject);

            dragGhost = null;
        }

        void DiscardSelectedItem(int amount)
        {
            if (selectedSlotIndex < 0 || itemDropper == null || playerInventory == null)
                return;

            var slot = playerInventory.GetSlot(selectedSlotIndex);
            if (slot.IsEmpty)
            {
                selectedSlotIndex = -1;
                ClearItemDetails();
                RefreshDiscardButtons();
                return;
            }

            var dropAmount = amount < 0 ? slot.quantity : Mathf.Clamp(amount, 1, slot.quantity);
            if (!itemDropper.DropFromSlot(playerInventory, selectedSlotIndex, dropAmount))
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

            RefreshDiscardButtons();
        }

        void RefreshDiscardButtons()
        {
            var hasSelection = selectedSlotIndex >= 0 &&
                               playerInventory != null &&
                               !playerInventory.GetSlot(selectedSlotIndex).IsEmpty;

            if (discardButton != null)
                discardButton.interactable = hasSelection;

            if (discardAllButton != null)
                discardAllButton.interactable = hasSelection;
        }

        void ClearItemDetails()
        {
            if (itemNameText != null)
                itemNameText.text = string.Empty;

            if (itemDescriptionText != null)
                itemDescriptionText.text = string.Empty;

            if (itemMetaText != null)
                itemMetaText.text = string.Empty;
        }

        void ShowItemDetails(InventorySlot slot)
        {
            if (slot.IsEmpty)
            {
                ClearItemDetails();
                return;
            }

            if (itemNameText != null)
                itemNameText.text = slot.item.DisplayName;

            if (itemMetaText != null)
                itemMetaText.text = BuildMetaText(slot);

            if (itemDescriptionText != null)
            {
                var description = slot.item.Description;
                if (itemMetaText == null)
                    description = $"{BuildMetaText(slot)}\n{description}";

                itemDescriptionText.text = description;
            }
        }

        static string BuildMetaText(InventorySlot slot)
        {
            var typeLabel = slot.item.ItemType.ToString();
            var quantityLabel = slot.item.IsUnique
                ? "Unique"
                : $"{slot.quantity} / {slot.item.MaxStackSize}";

            return $"Type: {typeLabel}\nQuantity: {quantityLabel}";
        }

        void OnDestroy()
        {
            if (discardButton != null)
                discardButton.onClick.RemoveAllListeners();

            if (discardAllButton != null)
                discardAllButton.onClick.RemoveAllListeners();

            ClearDragGhost();
        }
    }
}

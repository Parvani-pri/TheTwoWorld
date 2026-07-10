using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TwoWorlds.Inventory
{
    public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text quantityText;
        [SerializeField] Button button;
        [SerializeField] Image backgroundImage;
        [SerializeField] Color selectedColor = new(1f, 0.92f, 0.55f, 1f);
        [SerializeField] float dragStartThreshold = 8f;

        InventorySlot slot;
        int slotIndex = -1;
        Color normalColor;
        bool isSelected;
        bool dragStarted;
        bool suppressClick;
        Vector2 pointerDownPosition;
        System.Action<int, InventorySlot> onSelected;
        System.Func<int, int, bool> onMoveRequested;
        System.Action<int, PointerEventData> onBeginDragRequested;
        System.Action<PointerEventData> onDragRequested;
        System.Action onEndDragRequested;

        void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClick);

                if (backgroundImage == null)
                    backgroundImage = button.targetGraphic as Image;
            }

            if (backgroundImage != null)
                normalColor = backgroundImage.color;
        }

        public void Bind(
            int index,
            InventorySlot boundSlot,
            bool selected,
            System.Action<int, InventorySlot> selectedCallback,
            System.Func<int, int, bool> moveRequestedCallback,
            System.Action<int, PointerEventData> beginDragCallback,
            System.Action<PointerEventData> dragCallback,
            System.Action endDragCallback)
        {
            slotIndex = index;
            slot = boundSlot;
            isSelected = selected;
            onSelected = selectedCallback;
            onMoveRequested = moveRequestedCallback;
            onBeginDragRequested = beginDragCallback;
            onDragRequested = dragCallback;
            onEndDragRequested = endDragCallback;
            Refresh();
        }

        void Refresh()
        {
            var hasItem = !slot.IsEmpty;

            if (iconImage != null)
            {
                iconImage.enabled = hasItem;
                iconImage.sprite = hasItem ? slot.item.Icon : null;
            }

            if (quantityText != null)
            {
                var showQuantity = hasItem && slot.quantity > 1;
                quantityText.gameObject.SetActive(showQuantity);
                quantityText.text = showQuantity ? slot.quantity.ToString() : string.Empty;
            }

            if (backgroundImage != null)
                backgroundImage.color = isSelected ? selectedColor : normalColor;
        }

        void HandleClick()
        {
            if (suppressClick || slot.IsEmpty)
                return;

            onSelected?.Invoke(slotIndex, slot);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (slot.IsEmpty)
                return;

            pointerDownPosition = eventData.position;
            dragStarted = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (slot.IsEmpty)
                return;

            if (!dragStarted)
            {
                if (Vector2.Distance(eventData.position, pointerDownPosition) < dragStartThreshold)
                    return;

                dragStarted = true;
                suppressClick = true;
                onBeginDragRequested?.Invoke(slotIndex, eventData);
            }

            onDragRequested?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragStarted = false;
            onEndDragRequested?.Invoke();
            StartCoroutine(ResetSuppressClickNextFrame());
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            var draggedSlot = eventData.pointerDrag.GetComponent<InventorySlotUI>();
            if (draggedSlot == null || draggedSlot.slotIndex < 0 || slotIndex < 0)
                return;

            onMoveRequested?.Invoke(draggedSlot.slotIndex, slotIndex);
        }

        System.Collections.IEnumerator ResetSuppressClickNextFrame()
        {
            yield return null;
            suppressClick = false;
        }
    }
}

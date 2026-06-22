using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.Inventory
{
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text quantityText;
        [SerializeField] Button button;

        InventorySlot slot;
        System.Action<InventorySlot> onSelected;

        void Awake()
        {
            if (button != null)
                button.onClick.AddListener(HandleClick);
        }

        public void Bind(InventorySlot boundSlot, System.Action<InventorySlot> selectedCallback)
        {
            slot = boundSlot;
            onSelected = selectedCallback;
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
        }

        void HandleClick()
        {
            if (!slot.IsEmpty)
                onSelected?.Invoke(slot);
        }
    }
}

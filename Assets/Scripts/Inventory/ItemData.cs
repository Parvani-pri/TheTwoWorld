using UnityEngine;

namespace TwoWorlds.Inventory
{
    public enum ItemType
    {
        Generic,
        Key,
        Mask,
        Consumable,
        Quest
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "Two Worlds/Item Data")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] string itemId;
        [SerializeField] string displayName;
        [TextArea(2, 4)]
        [SerializeField] string description;
        [SerializeField] Sprite icon;
        [SerializeField] ItemType itemType = ItemType.Generic;
        [SerializeField] int maxStackSize = 99;
        [SerializeField] bool isUnique;

        public string ItemId => string.IsNullOrWhiteSpace(itemId) ? name : itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public ItemType ItemType => itemType;
        public int MaxStackSize => Mathf.Max(1, maxStackSize);
        public bool IsUnique => isUnique;
    }
}

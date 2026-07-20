using System.Collections;
using TwoWorlds.Core;
using TwoWorlds.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace XuFu.MaskSystem
{
    [DefaultExecutionOrder(1000)]
    public class MaskEquipGate : MonoBehaviour
    {
        public const string BoqiItemId = "008";
        public const string ZhongKuiItemId = "009";

        public static MaskEquipGate Instance { get; private set; }

        [SerializeField] MaskController maskController;
        [SerializeField] PlayerInventory playerInventory;
        [SerializeField] PlayerAttackUI attackUI;
        [SerializeField] MaskItem boqiCombatMask;
        [SerializeField] MaskItem zhongKuiCombatMask;
        [SerializeField] MaskItem blankMask;
        [SerializeField] ItemData boqiInventoryItem;
        [SerializeField] ItemData zhongKuiInventoryItem;
        [SerializeField] Sprite lockedMaskIcon;
        [SerializeField] EventTrigger mask1Trigger;
        [SerializeField] EventTrigger mask2Trigger;

        void Awake()
        {
            Instance = this;
            ResolveReferences();
            ResolveLockedMaskIcon();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void OnEnable()
        {
            GameEvents.InventoryChanged += OnInventoryChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(InitializeWhenReady());
        }

        void OnDisable()
        {
            GameEvents.InventoryChanged -= OnInventoryChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void Start()
        {
            RefreshAvailability();
        }

        void OnSceneLoaded(Scene _, LoadSceneMode __)
        {
            StartCoroutine(RefreshAfterSceneLoad());
        }

        IEnumerator RefreshAfterSceneLoad()
        {
            yield return null;
            RefreshAvailability();
        }

        void OnInventoryChanged(PlayerInventory inventory)
        {
            if (playerInventory == null)
                playerInventory = inventory;

            if (playerInventory == null || inventory == playerInventory)
                RefreshAvailability();
        }

        IEnumerator InitializeWhenReady()
        {
            const int maxFrames = 10;
            for (var i = 0; i < maxFrames; i++)
            {
                ResolveReferences();

                if (playerInventory != null && attackUI != null && maskController != null)
                    break;

                yield return null;
            }

            if (maskController != null)
                maskController.SetEquipGate(this);

            RefreshAvailability();
        }

        void ResolveReferences()
        {
            if (maskController == null)
                maskController = FindFirstObjectByType<MaskController>();

            if (playerInventory == null)
                playerInventory = FindFirstObjectByType<PlayerInventory>();

            if (attackUI == null)
                attackUI = GetComponent<PlayerAttackUI>();

            if (attackUI == null)
                attackUI = FindFirstObjectByType<PlayerAttackUI>(FindObjectsInactive.Include);

            attackUI?.EnsureMaskIconsResolved();

            if (mask1Trigger == null && attackUI != null)
                mask1Trigger = attackUI.Mask1Trigger;

            if (mask2Trigger == null && attackUI != null)
                mask2Trigger = attackUI.Mask2Trigger;

            ResolveInventoryItems();
        }

        void ResolveInventoryItems()
        {
            var catalog = Resources.Load<ItemDataCatalog>("ItemDataCatalog");
            if (catalog == null)
                return;

            if (boqiInventoryItem == null)
                catalog.TryGetItem(BoqiItemId, out boqiInventoryItem);

            if (zhongKuiInventoryItem == null)
                catalog.TryGetItem(ZhongKuiItemId, out zhongKuiInventoryItem);
        }

        void ResolveLockedMaskIcon()
        {
            if (lockedMaskIcon != null)
                return;

            lockedMaskIcon = Resources.Load<Sprite>("UI/wuxia-no-mask-icon-glyph");
        }

        public void TryEquipBoqiMask() => TryEquipMask(boqiCombatMask);

        public void TryEquipZhongKuiMask() => TryEquipMask(zhongKuiCombatMask);

        public void TryEquipBlankMask()
        {
            if (maskController == null)
                return;

            maskController.EquipMask(blankMask);
        }

        public void TryEquipMask(MaskItem mask)
        {
            if (maskController == null || mask == null)
                return;

            if (!IsMaskOwned(mask))
                return;

            maskController.EquipMask(mask);
        }

        public bool IsMaskOwned(MaskItem mask)
        {
            if (mask == null || mask == blankMask)
                return true;

            if (playerInventory == null)
                return false;

            if (boqiCombatMask != null && mask == boqiCombatMask)
                return playerInventory.HasItemId(BoqiItemId);

            if (zhongKuiCombatMask != null && mask == zhongKuiCombatMask)
                return playerInventory.HasItemId(ZhongKuiItemId);

            return false;
        }

        public void RefreshAvailability()
        {
            ResolveReferences();
            ResolveLockedMaskIcon();

            var hasBoqi = boqiCombatMask != null && IsMaskOwned(boqiCombatMask);
            var hasZhongKui = zhongKuiCombatMask != null && IsMaskOwned(zhongKuiCombatMask);

            if (attackUI != null)
            {
                attackUI.SetMaskSlotAvailable(1, hasBoqi, lockedMaskIcon);
                attackUI.SetMaskSlotAvailable(2, hasZhongKui, lockedMaskIcon);
            }

            SetTriggerEnabled(mask1Trigger, hasBoqi);
            SetTriggerEnabled(mask2Trigger, hasZhongKui);
            EnsureValidEquippedMask(hasBoqi, hasZhongKui);
        }

        void EnsureValidEquippedMask(bool hasBoqi, bool hasZhongKui)
        {
            if (maskController == null)
                return;

            var equipped = maskController.EquippedMask;
            if (equipped == null || equipped == blankMask)
            {
                if (equipped == null)
                    maskController.EquipMask(blankMask);

                return;
            }

            if (equipped == boqiCombatMask && !hasBoqi)
                maskController.EquipMask(blankMask);
            else if (equipped == zhongKuiCombatMask && !hasZhongKui)
                maskController.EquipMask(blankMask);
        }

        static void SetTriggerEnabled(EventTrigger trigger, bool enabled)
        {
            if (trigger != null)
                trigger.enabled = enabled;
        }
    }
}

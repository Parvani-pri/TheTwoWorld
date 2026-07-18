using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TwoWorlds.Inventory
{
    [Serializable]
    public struct SavedInventorySlotEntry
    {
        public string itemId;
        public int quantity;
    }

    public class PlayerInventoryPersistence : MonoBehaviour
    {
        public static PlayerInventoryPersistence Instance { get; private set; }

        [SerializeField] ItemDataCatalog itemCatalog;

        readonly List<SavedInventorySlotEntry> savedSlots = new();
        bool hasSavedState;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap() => EnsureInstance();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            ResolveCatalog();
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            if (Instance == this)
                Instance = null;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void OnSceneUnloaded(Scene _) => CaptureCurrentInventory();

        void OnSceneLoaded(Scene _, LoadSceneMode __) => TryRestoreCurrentInventory();

        public static void CaptureBeforeSceneLoad()
        {
            EnsureInstance();
            Instance?.CaptureCurrentInventory();
        }

        public void CaptureCurrentInventory()
        {
            var inventory = FindFirstObjectByType<PlayerInventory>();
            if (inventory == null)
                return;

            savedSlots.Clear();
            savedSlots.AddRange(inventory.CapturePersistentState());
            hasSavedState = true;
        }

        public void TryRestoreCurrentInventory()
        {
            if (!hasSavedState)
                return;

            var inventory = FindFirstObjectByType<PlayerInventory>();
            if (inventory == null)
                return;

            inventory.ApplyPersistentState(savedSlots, itemCatalog);
        }

        static void EnsureInstance()
        {
            if (Instance != null)
                return;

            var existing = FindFirstObjectByType<PlayerInventoryPersistence>();
            if (existing != null)
            {
                Instance = existing;
                return;
            }

            var go = new GameObject(nameof(PlayerInventoryPersistence));
            go.AddComponent<PlayerInventoryPersistence>();
        }

        void ResolveCatalog()
        {
            if (itemCatalog != null)
                return;

            itemCatalog = Resources.Load<ItemDataCatalog>("ItemDataCatalog");
        }
    }
}

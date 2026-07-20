using TwoWorlds.Core;
using TwoWorlds.Dialogue;
using TwoWorlds.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TwoWorlds.Progress
{
    public class EnterYinReadinessSession : MonoBehaviour
    {
        [SerializeField] EnterYinReadinessUI readinessUI;
        [SerializeField] GameProgress gameProgress;
        [SerializeField] PlayerInventory playerInventory;
        [SerializeField] CharacterPortraitDatabase portraitDatabase;
        [SerializeField] string speakerName = "小妹";
        [SerializeField] Sprite speakerPortrait;
        [TextArea(2, 4)]
        [SerializeField] string promptMessage = "許負，你準備好入陰了嗎？";
        [TextArea(2, 4)]
        [SerializeField] string blockedMessage = "你還沒有準備好面具，準備好再出發。";
        [SerializeField] string readyButtonLabel = "準備好了";
        [SerializeField] string notReadyButtonLabel = "沒準備好";

        bool isActive;
        int pendingChapter;

        public bool IsActive => isActive;

        public static EnterYinReadinessSession Instance { get; private set; }

        public static EnterYinReadinessSession FindInstance()
        {
            if (Instance != null)
                return Instance;

            return FindFirstObjectByType<EnterYinReadinessSession>(FindObjectsInactive.Include);
        }

        void Awake()
        {
            Instance = this;
            ResolveReferences();
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            RefreshUIBindings();
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnbindInventoryChanged();
            UnbindUIEvents();

            if (isActive)
                EndSession();

            if (Instance == this)
                Instance = null;
        }

        void OnSceneLoaded(Scene _, LoadSceneMode __) => RefreshUIBindings();

        void RefreshUIBindings()
        {
            UnbindUIEvents();
            ResolveReferences();
            BindUIEvents();
        }

        void ResolveReferences()
        {
            readinessUI = FindFirstObjectByType<EnterYinReadinessUI>(FindObjectsInactive.Include);

            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();

            if (playerInventory == null)
                playerInventory = FindFirstObjectByType<PlayerInventory>();
        }

        void BindUIEvents()
        {
            if (readinessUI == null)
                return;

            readinessUI.ReadySelected += OnReadySelected;
            readinessUI.NotReadySelected += OnNotReadySelected;
        }

        void UnbindUIEvents()
        {
            if (readinessUI == null)
                return;

            readinessUI.ReadySelected -= OnReadySelected;
            readinessUI.NotReadySelected -= OnNotReadySelected;
        }

        void BindInventoryChanged()
        {
            GameEvents.InventoryChanged -= OnInventoryChanged;
            GameEvents.InventoryChanged += OnInventoryChanged;
        }

        void UnbindInventoryChanged()
        {
            GameEvents.InventoryChanged -= OnInventoryChanged;
        }

        void OnInventoryChanged(PlayerInventory inventory)
        {
            if (!isActive)
                return;

            if (playerInventory == null)
                playerInventory = inventory;

            RefreshPresentation();
        }

        public bool IsAvailable()
        {
            if (isActive)
                return false;

            ResolveReferences();

            return gameProgress != null && gameProgress.IsEnterYinReadinessWindowOpen();
        }

        public void ShowPrompt()
        {
            ResolveReferences();

            if (readinessUI == null)
            {
                Debug.LogError("[EnterYinReadinessSession] EnterYinReadinessUI is missing.");
                return;
            }

            if (gameProgress == null || !gameProgress.TryGetEnterYinReadinessChapter(out pendingChapter))
                return;

            DismissRoamingInterruptIfNeeded();
            isActive = true;
            BindInventoryChanged();
            readinessUI.SetHandlers(OnReadySelected, OnNotReadySelected);
            readinessUI.Show(speakerName, ResolvePortrait(), promptMessage, readyButtonLabel, notReadyButtonLabel);
            RefreshPresentation();
            GameEvents.RaiseDialogueStarted();
        }

        void RefreshPresentation()
        {
            if (!isActive || readinessUI == null)
                return;

            ResolveReferences();

            var result = EnterYinMaskRequirementChecker.Evaluate(pendingChapter, playerInventory);
            if (playerInventory == null && EnterYinMaskRequirements.GetRequiredItemIds(pendingChapter).Count > 0)
            {
                Debug.LogWarning("[EnterYinReadinessSession] PlayerInventory missing; blocking enter-yin readiness.");
            }

            var message = result.CanEnter ? promptMessage : blockedMessage;
            readinessUI.SetPresentation(message, result.CanEnter);
        }

        Sprite ResolvePortrait()
        {
            if (speakerPortrait != null)
                return speakerPortrait;

            return portraitDatabase != null
                ? portraitDatabase.GetPortrait(speakerName)
                : null;
        }

        void OnReadySelected()
        {
            if (!isActive || gameProgress == null)
                return;

            ResolveReferences();

            var result = EnterYinMaskRequirementChecker.Evaluate(pendingChapter, playerInventory);
            if (!result.CanEnter)
            {
                RefreshPresentation();
                return;
            }

            var chapter = pendingChapter;
            gameProgress.UnlockEnterYin(chapter);
            gameProgress.SetChapterProgress(chapter, ChapterSegment.EnterYin);

            isActive = false;
            pendingChapter = 0;
            UnbindInventoryChanged();
            HideReadinessUI();

            var transition = EnterYinPaperBurnTransition.FindInstance();
            if (transition != null)
            {
                transition.Play(chapter, OnPaperBurnTransitionComplete);
                return;
            }

            Debug.LogWarning("[EnterYinReadinessSession] EnterYinPaperBurnTransition missing; loading yin level immediately.");

            GameEvents.RaiseDialogueEnded();
            SceneFlow.LoadYinLevel(chapter);
        }

        void OnPaperBurnTransitionComplete(int chapter)
        {
            GameEvents.RaiseDialogueEnded();
            SceneFlow.LoadYinLevel(chapter);
        }

        void OnNotReadySelected()
        {
            if (!isActive)
                return;

            EndSession();
        }

        void EndSession()
        {
            isActive = false;
            pendingChapter = 0;
            UnbindInventoryChanged();
            HideReadinessUI();
            GameEvents.RaiseDialogueEnded();
        }

        void HideReadinessUI()
        {
            // Unity objects use overloaded ==; avoid ?. which misses destroyed references.
            if (readinessUI == null)
                return;

            readinessUI.SetHandlers(null, null);
            readinessUI.Hide();
        }

        static void DismissRoamingInterruptIfNeeded()
        {
            var interrupter = FindFirstObjectByType<TwoWorlds.RoamingNpc.RoamingNpcInterrupter>();
            interrupter?.DismissInterrupt();
        }
    }
}

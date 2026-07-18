using TwoWorlds.Core;
using TwoWorlds.Dialogue;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TwoWorlds.Progress
{
    public class EnterYinReadinessSession : MonoBehaviour
    {
        [SerializeField] EnterYinReadinessUI readinessUI;
        [SerializeField] GameProgress gameProgress;
        [SerializeField] CharacterPortraitDatabase portraitDatabase;
        [SerializeField] string speakerName = "小妹";
        [SerializeField] Sprite speakerPortrait;
        [TextArea(2, 4)]
        [SerializeField] string promptMessage = "许负，你准备好入阴了吗？";
        [SerializeField] string readyButtonLabel = "准备好了";
        [SerializeField] string notReadyButtonLabel = "没准备好";

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
            readinessUI.SetHandlers(OnReadySelected, OnNotReadySelected);
            readinessUI.Show(speakerName, ResolvePortrait(), promptMessage, readyButtonLabel, notReadyButtonLabel);
            GameEvents.RaiseDialogueStarted();
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

            var chapter = pendingChapter;
            gameProgress.UnlockEnterYin(chapter);
            gameProgress.SetChapterProgress(chapter, ChapterSegment.EnterYin);
            EndSession();
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
            readinessUI?.SetHandlers(null, null);
            readinessUI?.Hide();
            GameEvents.RaiseDialogueEnded();
        }

        static void DismissRoamingInterruptIfNeeded()
        {
            var interrupter = FindFirstObjectByType<TwoWorlds.RoamingNpc.RoamingNpcInterrupter>();
            interrupter?.DismissInterrupt();
        }
    }
}

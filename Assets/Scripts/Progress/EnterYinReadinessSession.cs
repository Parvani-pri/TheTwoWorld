using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Progress
{
    public class EnterYinReadinessSession : MonoBehaviour
    {
        [SerializeField] EnterYinReadinessUI readinessUI;
        [SerializeField] GameProgress gameProgress;
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

            if (readinessUI == null)
                readinessUI = FindFirstObjectByType<EnterYinReadinessUI>(FindObjectsInactive.Include);

            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        void OnEnable()
        {
            if (readinessUI == null)
                return;

            readinessUI.ReadySelected += OnReadySelected;
            readinessUI.NotReadySelected += OnNotReadySelected;
        }

        void OnDisable()
        {
            if (readinessUI == null)
                return;

            readinessUI.ReadySelected -= OnReadySelected;
            readinessUI.NotReadySelected -= OnNotReadySelected;

            if (isActive)
                EndSession();

            if (Instance == this)
                Instance = null;
        }

        public bool IsAvailable()
        {
            if (isActive)
                return false;

            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();

            return gameProgress != null && gameProgress.IsEnterYinReadinessWindowOpen();
        }

        public void ShowPrompt()
        {
            if (readinessUI == null)
            {
                Debug.LogError("[EnterYinReadinessSession] EnterYinReadinessUI is missing.");
                return;
            }

            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();

            if (gameProgress == null || !gameProgress.TryGetEnterYinReadinessChapter(out pendingChapter))
                return;

            DismissRoamingInterruptIfNeeded();
            isActive = true;
            readinessUI.SetHandlers(OnReadySelected, OnNotReadySelected);
            readinessUI.Show(speakerName, speakerPortrait, promptMessage, readyButtonLabel, notReadyButtonLabel);
            GameEvents.RaiseDialogueStarted();
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

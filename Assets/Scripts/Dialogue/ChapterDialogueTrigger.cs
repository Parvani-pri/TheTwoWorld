using TwoWorlds.Core;
using TwoWorlds.Dialogue;
using TwoWorlds.Progress;
using UnityEngine;

namespace TwoWorlds.Dialogue
{
    /// <summary>
    /// Plays one chapter segment dialogue (1101/1102/1103/1104 pattern) when its unlock rules are met.
    /// Add a Collider2D only when this object should be approached and interacted with manually.
    /// </summary>
    public class ChapterDialogueTrigger : MonoBehaviour, IInteractable, IScriptDialogueSource
    {
        [SerializeField] DialogueCsvLibrary csvLibrary;
        [SerializeField] GameProgress gameProgress;
        [SerializeField] int chapterNumber = 1;
        [SerializeField] ChapterSegment segment = ChapterSegment.PreBattle;
        [SerializeField] bool playOnce = true;
        [SerializeField] bool stageMainLobbyActorsOnStart;
        [SerializeField] bool stageXiaomeiOnStart = true;
        [SerializeField] bool faceMainLobbyActorsLeftOnStart;
        [SerializeField] string promptText = "對話";

        void Awake()
        {
            ResolveGameProgress();
        }

        GameProgress Progress
        {
            get
            {
                if (gameProgress == null)
                    ResolveGameProgress();

                return gameProgress;
            }
        }

        void ResolveGameProgress()
        {
            gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        public bool CanInteract(GameObject interactor) => IsDialogueAvailable(interactor);

        public bool IsDialogueAvailable(GameObject interactor)
        {
            if (csvLibrary == null || Progress == null)
                return false;

            var dialogueId = ChapterProgressCatalog.GetDialogueId(chapterNumber, segment);
            if (playOnce && Progress.HasDialogue(dialogueId))
                return false;

            return Progress.CanStartChapterDialogue(chapterNumber, segment) &&
                   csvLibrary.TryGetDialogue(dialogueId, out _);
        }

        public void Interact(GameObject interactor) => TriggerDialogue(interactor);

        public void TriggerDialogue(GameObject interactor)
        {
            var dialogueId = ChapterProgressCatalog.GetDialogueId(chapterNumber, segment);

            if (!IsDialogueAvailable(interactor))
            {
                Debug.LogWarning(
                    $"[ChapterDialogueTrigger] Dialogue not available: {dialogueId} (chapter={chapterNumber}, segment={segment}).");
                return;
            }

            if (!csvLibrary.TryGetDialogue(dialogueId, out var sessionData))
            {
                Debug.LogWarning($"[ChapterDialogueTrigger] Missing CSV dialogue: {dialogueId}");
                return;
            }

            if (stageMainLobbyActorsOnStart)
                DialogueAnchorCommands.StageMainLobbyXuFuAndXiaomei(stageXiaomeiOnStart, faceMainLobbyActorsLeftOnStart);

            if (DialogueManager.Instance != null)
                DialogueManager.Instance.StartDialogue(sessionData, interactor);
            else
                Debug.LogError("[ChapterDialogueTrigger] DialogueManager.Instance is null.");
        }

        public string GetPromptText() => promptText;
    }
}

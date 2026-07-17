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
        [SerializeField] string promptText = "对话";

        void Awake()
        {
            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        public bool CanInteract(GameObject interactor) => IsDialogueAvailable(interactor);

        public bool IsDialogueAvailable(GameObject interactor)
        {
            if (csvLibrary == null || gameProgress == null)
                return false;

            var dialogueId = ChapterProgressCatalog.GetDialogueId(chapterNumber, segment);
            if (playOnce && gameProgress.HasDialogue(dialogueId))
                return false;

            return gameProgress.CanStartChapterDialogue(chapterNumber, segment) &&
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

            if (DialogueManager.Instance != null)
                DialogueManager.Instance.StartDialogue(sessionData, interactor);
            else
                Debug.LogError("[ChapterDialogueTrigger] DialogueManager.Instance is null.");
        }

        public string GetPromptText() => promptText;
    }
}

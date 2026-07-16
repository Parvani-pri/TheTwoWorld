using System.Collections;
using TwoWorlds.Core;
using TwoWorlds.Inventory;
using UnityEngine;

namespace TwoWorlds.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [SerializeField] DialogueUI dialogueUI;
        [SerializeField] CharacterPortraitDatabase portraitDatabase;
        [SerializeField] float charactersPerSecond = 40f;

        DialogueSessionData currentDialogue;
        int currentLineIndex;
        bool isPlaying;
        bool lineFinished;
        Coroutine typingRoutine;
        GameObject lastInteractor;

        public bool IsPlaying => isPlaying;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void StartDialogue(DialogueData dialogue, GameObject interactor) =>
            StartDialogue(DialogueSessionData.FromAsset(dialogue), interactor);

        public void StartDialogue(DialogueSessionData dialogue, GameObject interactor)
        {
            if (dialogue == null || dialogue.Lines == null || dialogue.Lines.Count == 0 || isPlaying)
                return;

            lastInteractor = interactor;
            currentDialogue = dialogue;
            currentLineIndex = 0;
            isPlaying = true;
            GameEvents.RaiseDialogueStarted();

            if (dialogueUI != null)
                dialogueUI.Show();

            ShowCurrentLine();
        }

        public void AdvanceDialogue()
        {
            if (!isPlaying)
                return;

            if (!lineFinished)
            {
                CompleteCurrentLineInstantly();
                return;
            }

            currentLineIndex++;
            if (currentLineIndex >= currentDialogue.Lines.Count)
            {
                EndDialogue();
                return;
            }

            ShowCurrentLine();
        }

        void ShowCurrentLine()
        {
            lineFinished = false;
            var line = currentDialogue.Lines[currentLineIndex];

            if (typingRoutine != null)
                StopCoroutine(typingRoutine);

            typingRoutine = StartCoroutine(TypeLine(line));
        }

        IEnumerator TypeLine(DialogueLine line)
        {
            var portrait = line.Portrait != null
                ? line.Portrait
                : portraitDatabase?.GetPortrait(line.SpeakerName);

            dialogueUI?.SetSpeaker(line.SpeakerName, portrait);
            dialogueUI?.SetBodyText(string.Empty);

            var fullText = line.Text ?? string.Empty;
            if (charactersPerSecond <= 0f)
            {
                dialogueUI?.SetBodyText(fullText);
                lineFinished = true;
                yield break;
            }

            var visibleCount = 0;
            var delay = 1f / charactersPerSecond;

            while (visibleCount < fullText.Length)
            {
                visibleCount++;
                dialogueUI?.SetBodyText(fullText.Substring(0, visibleCount));
                yield return new WaitForSecondsRealtime(delay);
            }

            lineFinished = true;
        }

        void CompleteCurrentLineInstantly()
        {
            if (typingRoutine != null)
            {
                StopCoroutine(typingRoutine);
                typingRoutine = null;
            }

            var line = currentDialogue.Lines[currentLineIndex];
            dialogueUI?.SetBodyText(line.Text);
            lineFinished = true;
        }

        void EndDialogue()
        {
            TryGrantReward();

            var endedDialogueId = currentDialogue?.DialogueId ?? string.Empty;
            var progressNote = currentDialogue?.ProgressNote ?? string.Empty;
            var interactor = lastInteractor;

            isPlaying = false;
            currentDialogue = null;
            lineFinished = false;
            lastInteractor = null;

            if (dialogueUI != null)
                dialogueUI.Hide();

            if (interactor != null)
                GameEvents.RaiseDialogueEnded(new DialogueEndInfo(interactor, endedDialogueId, progressNote));
            else
                GameEvents.RaiseDialogueEnded();
        }

        void TryGrantReward()
        {
            if (currentDialogue?.RewardItem == null)
                return;

            var inventory = FindFirstObjectByType<PlayerInventory>();
            if (inventory == null)
                return;

            inventory.AddItem(currentDialogue.RewardItem, currentDialogue.RewardAmount);
        }
    }
}

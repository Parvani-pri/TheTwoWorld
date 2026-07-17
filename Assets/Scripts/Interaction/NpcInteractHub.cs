using TwoWorlds.AI;
using TwoWorlds.Core;
using TwoWorlds.Dialogue;
using TwoWorlds.Progress;
using UnityEngine;

namespace TwoWorlds.Interaction
{
    [RequireComponent(typeof(Collider2D))]
    public class NpcInteractHub : MonoBehaviour, IInteractable
    {
        [Header("Capabilities")]
        [SerializeField] ChapterDialogueTrigger chapterDialogueTrigger;
        [SerializeField] StagedDialogueTrigger stagedDialogueTrigger;
        [SerializeField] DialogueTrigger dialogueTrigger;
        [SerializeField] AIChatTrigger aiChatTrigger;
        [SerializeField] AIChatSession chatSession;
        [SerializeField] GameProgress gameProgress;

        [Header("Presentation")]
        [SerializeField] string promptText = "交谈";

        [Header("Post Dialogue AI")]
        [SerializeField] bool enterAiAfterScriptDialogue = true;
        [TextArea(2, 4)]
        [SerializeField] string postDialogueOpeningMessage = "有什么想问的吗？选一个话题吧。";

        GameObject pendingInteractor;
        bool waitingForScriptDialogueEnd;

        void Awake()
        {
            if (chapterDialogueTrigger == null)
                chapterDialogueTrigger = GetComponent<ChapterDialogueTrigger>();

            if (stagedDialogueTrigger == null)
                stagedDialogueTrigger = GetComponent<StagedDialogueTrigger>();

            if (dialogueTrigger == null)
                dialogueTrigger = GetComponent<DialogueTrigger>();

            if (aiChatTrigger == null)
                aiChatTrigger = GetComponent<AIChatTrigger>();

            if (chatSession == null)
                chatSession = FindFirstObjectByType<AIChatSession>();

            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        void OnEnable() => GameEvents.ScriptDialogueEnded += OnScriptDialogueEnded;

        void OnDisable()
        {
            GameEvents.ScriptDialogueEnded -= OnScriptDialogueEnded;
            waitingForScriptDialogueEnd = false;
            pendingInteractor = null;
        }

        public bool CanInteract(GameObject interactor)
        {
            if (IsBusy())
                return false;

            return IsDialogueAvailable(interactor) || IsAIChatAvailable(interactor);
        }

        public void Interact(GameObject interactor)
        {
            if (IsBusy())
                return;

            pendingInteractor = interactor;

            if (IsDialogueAvailable(interactor))
            {
                StartScriptDialogue(interactor);
                return;
            }

            if (IsAIChatAvailable(interactor))
                StartAIChat(interactor);
        }

        public string GetPromptText() => promptText;

        bool IsDialogueAvailable(GameObject interactor) =>
            ResolveDialogueSource(interactor)?.IsDialogueAvailable(interactor) ?? false;

        bool IsAIChatAvailable(GameObject interactor) =>
            aiChatTrigger != null && aiChatTrigger.IsAIChatAvailable(interactor);

        bool IsBusy()
        {
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying)
                return true;

            if (chatSession == null)
                chatSession = FindFirstObjectByType<AIChatSession>();

            return chatSession != null && chatSession.IsActive;
        }

        void StartScriptDialogue(GameObject interactor)
        {
            var source = ResolveDialogueSource(interactor);
            if (source == null)
                return;

            pendingInteractor = interactor;
            waitingForScriptDialogueEnd = ShouldEnterAiAfterScriptDialogue(interactor);
            source.TriggerDialogue(interactor);
        }

        IScriptDialogueSource ResolveDialogueSource(GameObject interactor)
        {
            if (chapterDialogueTrigger != null && chapterDialogueTrigger.IsDialogueAvailable(interactor))
                return chapterDialogueTrigger;

            if (stagedDialogueTrigger != null && stagedDialogueTrigger.IsDialogueAvailable(interactor))
                return stagedDialogueTrigger;

            if (dialogueTrigger != null && dialogueTrigger.IsDialogueAvailable(interactor))
                return dialogueTrigger;

            return null;
        }

        bool ShouldEnterAiAfterScriptDialogue(GameObject interactor)
        {
            if (!enterAiAfterScriptDialogue || !IsAIChatAvailable(interactor))
                return false;

            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();

            return gameProgress == null || gameProgress.IsAiDialogueWindowOpen();
        }

        void StartAIChat(GameObject interactor, string openingMessageOverride = null)
        {
            if (aiChatTrigger == null)
                return;

            waitingForScriptDialogueEnd = false;
            pendingInteractor = interactor;
            aiChatTrigger.TriggerAIChat(interactor, openingMessageOverride);
        }

        void OnScriptDialogueEnded(DialogueEndInfo info)
        {
            if (!waitingForScriptDialogueEnd)
                return;

            waitingForScriptDialogueEnd = false;

            if (!info.HasInteractor || info.Interactor != pendingInteractor)
                return;

            if (!IsAIChatAvailable(info.Interactor))
                return;

            StartAIChat(info.Interactor, postDialogueOpeningMessage);
        }
    }
}

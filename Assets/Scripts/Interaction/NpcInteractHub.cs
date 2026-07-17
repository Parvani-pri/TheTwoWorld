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
        [SerializeField] EnterYinReadinessTrigger enterYinReadinessTrigger;
        [SerializeField] EnterYinReadinessSession enterYinReadinessSession;
        [SerializeField] GameProgress gameProgress;

        [Header("Presentation")]
        [SerializeField] string promptText = "交谈";

        void Awake()
        {
            if (chapterDialogueTrigger == null)
                chapterDialogueTrigger = GetComponent<ChapterDialogueTrigger>();

            if (stagedDialogueTrigger == null)
                stagedDialogueTrigger = GetComponent<StagedDialogueTrigger>();

            if (dialogueTrigger == null)
                dialogueTrigger = GetComponent<DialogueTrigger>();

            if (enterYinReadinessTrigger == null)
                enterYinReadinessTrigger = GetComponent<EnterYinReadinessTrigger>();

            if (enterYinReadinessSession == null)
                enterYinReadinessSession = EnterYinReadinessSession.FindInstance();

            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        public bool CanInteract(GameObject interactor)
        {
            if (IsBusy())
                return false;

            return IsDialogueAvailable(interactor) || IsEnterYinReadinessAvailable();
        }

        public void Interact(GameObject interactor)
        {
            if (IsBusy())
                return;

            if (IsEnterYinReadinessAvailable())
            {
                StartEnterYinReadiness();
                return;
            }

            if (IsDialogueAvailable(interactor))
                StartScriptDialogue(interactor);
        }

        public string GetPromptText() => promptText;

        bool IsDialogueAvailable(GameObject interactor) =>
            ResolveDialogueSource(interactor)?.IsDialogueAvailable(interactor) ?? false;

        bool IsEnterYinReadinessAvailable()
        {
            if (enterYinReadinessSession == null)
                enterYinReadinessSession = EnterYinReadinessSession.FindInstance();

            return enterYinReadinessSession != null && enterYinReadinessSession.IsAvailable();
        }

        bool IsBusy()
        {
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying)
                return true;

            if (enterYinReadinessSession == null)
                enterYinReadinessSession = EnterYinReadinessSession.FindInstance();

            return enterYinReadinessSession != null && enterYinReadinessSession.IsActive;
        }

        void StartScriptDialogue(GameObject interactor)
        {
            var source = ResolveDialogueSource(interactor);
            source?.TriggerDialogue(interactor);
        }

        IScriptDialogueSource ResolveDialogueSource(GameObject interactor)
        {
            foreach (var trigger in GetComponents<ChapterDialogueTrigger>())
            {
                if (trigger != null && trigger.IsDialogueAvailable(interactor))
                    return trigger;
            }

            if (stagedDialogueTrigger != null && stagedDialogueTrigger.IsDialogueAvailable(interactor))
                return stagedDialogueTrigger;

            if (dialogueTrigger != null && dialogueTrigger.IsDialogueAvailable(interactor))
                return dialogueTrigger;

            return null;
        }

        void StartEnterYinReadiness()
        {
            if (enterYinReadinessSession == null)
                enterYinReadinessSession = EnterYinReadinessSession.FindInstance();

            enterYinReadinessSession?.ShowPrompt();
        }
    }
}

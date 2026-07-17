using TwoWorlds.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TwoWorlds.RoamingNpc
{
    public class InterruptDialogueInputHandler : MonoBehaviour
    {
        [SerializeField] InputReader inputReader;
        [SerializeField] InterruptDialogueUI interruptDialogueUI;
        [SerializeField] RoamingNpcInterrupter interrupter;

        void Start()
        {
            if (inputReader == null)
                inputReader = FindFirstObjectByType<InputReader>();

            if (interruptDialogueUI == null)
                interruptDialogueUI = FindFirstObjectByType<InterruptDialogueUI>(FindObjectsInactive.Include);

            if (interrupter == null)
                interrupter = FindFirstObjectByType<RoamingNpcInterrupter>();

            if (inputReader?.InteractAction != null)
                inputReader.InteractAction.performed += OnAdvance;

            if (inputReader?.SubmitAction != null)
                inputReader.SubmitAction.performed += OnAdvance;
        }

        void OnDestroy()
        {
            if (inputReader?.InteractAction != null)
                inputReader.InteractAction.performed -= OnAdvance;

            if (inputReader?.SubmitAction != null)
                inputReader.SubmitAction.performed -= OnAdvance;
        }

        void OnAdvance(InputAction.CallbackContext _)
        {
            if (TwoWorlds.Dialogue.DialogueManager.Instance != null &&
                TwoWorlds.Dialogue.DialogueManager.Instance.IsPlaying)
                return;

            var chatSession = TwoWorlds.AI.AIChatSession.FindInstance();
            if (chatSession != null && chatSession.IsActive)
                return;

            var readinessSession = TwoWorlds.Progress.EnterYinReadinessSession.FindInstance();
            if (readinessSession != null && readinessSession.IsActive)
                return;

            if (interruptDialogueUI == null || !interruptDialogueUI.IsShowing)
                return;

            if (!interruptDialogueUI.LineFinished)
            {
                interruptDialogueUI.CompleteLineInstantly();
                return;
            }

            interrupter?.DismissInterrupt();
        }
    }
}

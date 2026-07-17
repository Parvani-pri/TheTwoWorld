using TwoWorlds.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TwoWorlds.RoamingNpc
{
    public class InterruptDialogueInputHandler : MonoBehaviour
    {
        [SerializeField] InputReader inputReader;
        [SerializeField] InterruptDialogueUI interruptDialogueUI;
        [SerializeField] RoamingNpcInterrupter interrupter;

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            RefreshBindings();
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnsubscribeInput();
        }

        void OnSceneLoaded(Scene _, LoadSceneMode __) => RefreshBindings();

        void RefreshBindings()
        {
            UnsubscribeInput();
            ResolveReferences();
            SubscribeInput();
        }

        void ResolveReferences()
        {
            if (inputReader == null)
                inputReader = FindFirstObjectByType<InputReader>();

            if (interruptDialogueUI == null)
                interruptDialogueUI = FindFirstObjectByType<InterruptDialogueUI>(FindObjectsInactive.Include);

            if (interrupter == null)
                interrupter = FindFirstObjectByType<RoamingNpcInterrupter>();
        }

        void SubscribeInput()
        {
            if (inputReader?.InteractAction != null)
                inputReader.InteractAction.performed += OnAdvance;

            if (inputReader?.SubmitAction != null)
                inputReader.SubmitAction.performed += OnAdvance;
        }

        void UnsubscribeInput()
        {
            if (inputReader?.InteractAction != null)
                inputReader.InteractAction.performed -= OnAdvance;

            if (inputReader?.SubmitAction != null)
                inputReader.SubmitAction.performed -= OnAdvance;
        }

        void OnAdvance(InputAction.CallbackContext _)
        {
            ResolveReferences();

            if (interruptDialogueUI == null || !interruptDialogueUI.IsShowing)
                return;

            if (TwoWorlds.Dialogue.DialogueManager.Instance != null &&
                TwoWorlds.Dialogue.DialogueManager.Instance.IsPlaying)
                return;

            var chatSession = TwoWorlds.AI.AIChatSession.FindInstance();
            if (chatSession != null && chatSession.IsActive)
                return;

            var readinessSession = TwoWorlds.Progress.EnterYinReadinessSession.FindInstance();
            if (readinessSession != null && readinessSession.IsActive)
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

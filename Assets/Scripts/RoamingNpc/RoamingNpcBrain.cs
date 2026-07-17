using UnityEngine;

namespace TwoWorlds.RoamingNpc
{
    public enum RoamingNpcState
    {
        Disabled,
        Roaming,
        Approaching,
        InterruptShowing,
        ExternalDialogue,
        Cooldown
    }

    public class RoamingNpcBrain : MonoBehaviour
    {
        [SerializeField] RoamingNpcController controller;
        [SerializeField] RoamingNpcInterrupter interrupter;
        [SerializeField] InterruptDialogueUI interruptDialogueUI;

        RoamingNpcState state = RoamingNpcState.Disabled;
        int scriptedBeatDepth;

        public RoamingNpcState State => state;
        public bool IsScriptedBeatActive => scriptedBeatDepth > 0;

        void Awake()
        {
            if (controller == null)
                controller = GetComponent<RoamingNpcController>();

            if (interrupter == null)
                interrupter = GetComponent<RoamingNpcInterrupter>();

            ResolveInterruptDialogueUI();
        }

        InterruptDialogueUI InterruptUI
        {
            get
            {
                if (interruptDialogueUI == null)
                    ResolveInterruptDialogueUI();

                return interruptDialogueUI;
            }
        }

        void ResolveInterruptDialogueUI()
        {
            if (interruptDialogueUI == null)
                interruptDialogueUI = FindFirstObjectByType<InterruptDialogueUI>(FindObjectsInactive.Include);
        }

        void Update()
        {
            if (RoamingNpcInterrupter.ShouldYieldToScriptOrAiChat())
            {
                if (state == RoamingNpcState.InterruptShowing ||
                    state == RoamingNpcState.Approaching)
                    interrupter?.DismissInterrupt();

                if (state != RoamingNpcState.ExternalDialogue)
                    SetState(RoamingNpcState.ExternalDialogue);

                return;
            }

            if (state == RoamingNpcState.InterruptShowing)
                return;

            if (IsExternalDialogueBlocking())
            {
                if (state != RoamingNpcState.ExternalDialogue)
                    SetState(RoamingNpcState.ExternalDialogue);
                return;
            }

            if (state == RoamingNpcState.ExternalDialogue)
            {
                SetState(RoamingNpcState.Roaming);
                controller?.WakeUpWander();
            }
        }

        public void SetState(RoamingNpcState newState)
        {
            if (state == newState)
                return;

            state = newState;
            ApplyState();
        }

        public void BeginScriptedBeat()
        {
            scriptedBeatDepth++;
            ApplyState();
        }

        public void EndScriptedBeat()
        {
            scriptedBeatDepth = Mathf.Max(0, scriptedBeatDepth - 1);
            ApplyState();
        }

        public bool AllowRoamingMovement() =>
            state == RoamingNpcState.Roaming || state == RoamingNpcState.Cooldown;

        public bool AllowInterruptLogic() =>
            state == RoamingNpcState.Roaming || state == RoamingNpcState.Cooldown;

        void ApplyState()
        {
            if (controller != null)
            {
                var allowMove = AllowRoamingMovement() ||
                                state == RoamingNpcState.Approaching ||
                                IsScriptedBeatActive;
                controller.SetMovementEnabled(allowMove);
            }

            if (interrupter != null)
                interrupter.SetInterruptEnabled(AllowInterruptLogic());
        }

        bool IsExternalDialogueBlocking()
        {
            if (InterruptUI != null && InterruptUI.IsShowing)
                return false;

            if (TwoWorlds.Dialogue.DialogueManager.Instance != null &&
                TwoWorlds.Dialogue.DialogueManager.Instance.IsPlaying)
                return true;

            var chatSession = TwoWorlds.AI.AIChatSession.FindInstance();
            if (chatSession != null && chatSession.IsActive)
                return true;

            var readinessSession = TwoWorlds.Progress.EnterYinReadinessSession.FindInstance();
            if (readinessSession != null && readinessSession.IsActive)
                return true;

            return TwoWorlds.Core.GameEvents.IsInventoryOpen;
        }
    }
}

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

        public RoamingNpcState State => state;

        void Awake()
        {
            if (controller == null)
                controller = GetComponent<RoamingNpcController>();

            if (interrupter == null)
                interrupter = GetComponent<RoamingNpcInterrupter>();

            if (interruptDialogueUI == null)
                interruptDialogueUI = FindFirstObjectByType<InterruptDialogueUI>(FindObjectsInactive.Include);
        }

        void Update()
        {
            if (state == RoamingNpcState.InterruptShowing)
                return;

            if (IsExternalDialogueBlocking())
            {
                if (state != RoamingNpcState.ExternalDialogue)
                    SetState(RoamingNpcState.ExternalDialogue);
                return;
            }

            if (state == RoamingNpcState.ExternalDialogue)
                SetState(RoamingNpcState.Roaming);
        }

        public void SetState(RoamingNpcState newState)
        {
            if (state == newState)
                return;

            state = newState;
            ApplyState();
        }

        public bool AllowRoamingMovement() =>
            state == RoamingNpcState.Roaming || state == RoamingNpcState.Cooldown;

        public bool AllowInterruptLogic() =>
            state == RoamingNpcState.Roaming || state == RoamingNpcState.Cooldown;

        void ApplyState()
        {
            if (controller != null)
                controller.SetMovementEnabled(AllowRoamingMovement() || state == RoamingNpcState.Approaching);

            if (interrupter != null)
                interrupter.SetInterruptEnabled(AllowInterruptLogic());
        }

        bool IsExternalDialogueBlocking()
        {
            if (interruptDialogueUI != null && interruptDialogueUI.IsShowing)
                return false;

            if (TwoWorlds.Dialogue.DialogueManager.Instance != null &&
                TwoWorlds.Dialogue.DialogueManager.Instance.IsPlaying)
                return true;

            var chatSession = TwoWorlds.AI.AIChatSession.FindInstance();
            if (chatSession != null && chatSession.IsActive)
                return true;

            return TwoWorlds.Core.GameEvents.IsInventoryOpen;
        }
    }
}

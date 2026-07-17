using System.Collections;
using TwoWorlds.AI;
using TwoWorlds.Inventory;
using TwoWorlds.Progress;
using UnityEngine;

namespace TwoWorlds.RoamingNpc
{
    public class RoamingNpcInterrupter : MonoBehaviour
    {
        [SerializeField] RoamingNpcConfig config;
        [SerializeField] RoamingNpcController controller;
        [SerializeField] RoamingNpcBrain brain;
        [SerializeField] InterruptDialogueUI interruptDialogueUI;
        [SerializeField] AIService aiService;
        [SerializeField] GameProgress gameProgress;
        [SerializeField] float postDialogueApproachDelay = 2f;
        [SerializeField] float interruptAiTimeoutSeconds = 12f;
        [Tooltip("After any of these dialogue IDs complete, roaming interrupt stays disabled.")]
        [SerializeField] string[] disableAfterDialogueIds;

        Transform playerTransform;
        float cooldownUntil;
        float approachGraceUntil;
        bool interruptLogicEnabled = true;
        int activeRequestId;
        bool waitingForAi;
        RoamingNpcState previousBrainState = RoamingNpcState.Disabled;
        Coroutine interruptTimeoutRoutine;

        void Awake()
        {
            if (controller == null)
                controller = GetComponent<RoamingNpcController>();

            if (brain == null)
                brain = GetComponent<RoamingNpcBrain>();

            if (interruptDialogueUI == null)
                interruptDialogueUI = FindFirstObjectByType<InterruptDialogueUI>(FindObjectsInactive.Include);

            if (aiService == null)
                aiService = AIService.Instance ?? FindFirstObjectByType<AIService>();

            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        void Start()
        {
            var inventory = FindFirstObjectByType<PlayerInventory>();
            if (inventory != null)
                playerTransform = inventory.transform;

            controller?.SetAnchorToCurrentPosition();
            UpdateUnlockState();
        }

        void Update()
        {
            UpdateUnlockState();
            TrackBrainTransitions();

            if (ShouldYieldToScriptOrAiChat())
            {
                AbortInterruptForExternalDialogue();
                return;
            }

            if (brain == null)
                return;

            if (brain.State == RoamingNpcState.InterruptShowing)
                return;

            if (brain.State == RoamingNpcState.Approaching)
            {
                UpdateApproaching();
                return;
            }

            if (!interruptLogicEnabled)
                return;

            if (brain.State == RoamingNpcState.Cooldown && Time.time >= cooldownUntil)
                brain.SetState(RoamingNpcState.Roaming);

            if (!brain.AllowInterruptLogic())
                return;

            if (waitingForAi)
                return;

            if (Time.time < cooldownUntil || Time.time < approachGraceUntil)
                return;

            if (!TryGetPlayerPlanarPosition(out var playerPosition))
                return;

            if (GetPlanarDistance(transform.position, playerPosition) > config.SenseRadius)
                return;

            BeginApproach(playerPosition);
        }

        void TrackBrainTransitions()
        {
            if (brain == null)
                return;

            if (previousBrainState == RoamingNpcState.ExternalDialogue &&
                brain.State == RoamingNpcState.Roaming)
            {
                approachGraceUntil = Time.time + postDialogueApproachDelay;
                controller?.WakeUpWander();
            }

            previousBrainState = brain.State;
        }

        public void SetInterruptEnabled(bool enabled)
        {
            interruptLogicEnabled = enabled;
        }

        public void DismissInterrupt()
        {
            waitingForAi = false;
            activeRequestId++;
            StopInterruptTimeout();

            interruptDialogueUI?.Hide();

            if (config != null)
                cooldownUntil = Time.time + config.InterruptCooldownSeconds;

            brain?.SetState(RoamingNpcState.Roaming);
            controller?.StopMoving();
            controller?.WakeUpWander();
        }

        void UpdateUnlockState()
        {
            if (brain == null || gameProgress == null)
                return;

            if (IsInterruptPermanentlyDisabled())
            {
                if (brain.State != RoamingNpcState.Disabled &&
                    brain.State != RoamingNpcState.ExternalDialogue &&
                    brain.State != RoamingNpcState.InterruptShowing)
                {
                    brain.SetState(RoamingNpcState.Disabled);
                }

                return;
            }

            if (!gameProgress.IsXiaomeiInterruptUnlocked())
            {
                if (brain.State != RoamingNpcState.Disabled &&
                    brain.State != RoamingNpcState.ExternalDialogue &&
                    brain.State != RoamingNpcState.InterruptShowing)
                {
                    brain.SetState(RoamingNpcState.Disabled);
                }

                return;
            }

            if (brain.State == RoamingNpcState.Disabled)
            {
                brain.SetState(RoamingNpcState.Roaming);
                controller?.WakeUpWander();
            }
        }

        bool IsInterruptPermanentlyDisabled()
        {
            if (disableAfterDialogueIds == null || gameProgress == null)
                return false;

            foreach (var dialogueId in disableAfterDialogueIds)
            {
                if (!string.IsNullOrWhiteSpace(dialogueId) && gameProgress.HasDialogue(dialogueId))
                    return true;
            }

            return false;
        }

        void BeginApproach(Vector3 playerPosition)
        {
            if (config == null || controller == null || brain == null)
                return;

            if (ShouldYieldToScriptOrAiChat())
                return;

            var direction = transform.position - playerPosition;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                direction = Vector3.forward;

            direction.Normalize();

            var target = playerPosition + direction * config.ApproachStopDistance;
            target.y = transform.position.y;

            controller.MoveTo(target);
            brain.SetState(RoamingNpcState.Approaching);
        }

        void UpdateApproaching()
        {
            if (config == null || controller == null || brain == null)
                return;

            if (waitingForAi)
                return;

            if (!TryGetPlayerPlanarPosition(out var playerPosition))
            {
                CancelApproach(config.FailedChaseCooldownSeconds);
                return;
            }

            if (GetPlanarDistance(transform.position, playerPosition) > config.ChaseCancelDistance)
            {
                CancelApproach(config.FailedChaseCooldownSeconds);
                return;
            }

            var reachedApproachPoint = controller.HasReachedTarget();
            var closeEnoughToPlayer = GetPlanarDistance(transform.position, playerPosition) <=
                                      config.ApproachStopDistance + config.ArriveThreshold;

            if (!reachedApproachPoint && !closeEnoughToPlayer)
                return;

            controller.StopMoving();
            RequestInterruptLine();
        }

        void CancelApproach(float cooldownSeconds)
        {
            controller?.StopMoving();
            cooldownUntil = Time.time + cooldownSeconds;
            brain?.SetState(RoamingNpcState.Roaming);
            controller?.WakeUpWander();
        }

        void RequestInterruptLine()
        {
            if (config == null || brain == null || waitingForAi)
                return;

            waitingForAi = true;
            brain.SetState(RoamingNpcState.Approaching);

            if (aiService == null)
            {
                ShowInterruptLine(config.GetRandomFallbackLine());
                return;
            }

            var inventory = playerTransform != null
                ? playerTransform.GetComponent<PlayerInventory>()
                : FindFirstObjectByType<PlayerInventory>();

            var requestId = ++activeRequestId;
            StartInterruptTimeout(requestId);
            aiService.AskInterruptLine(
                inventory,
                config.InterruptPersona,
                response => HandleInterruptSuccess(requestId, response),
                error => HandleInterruptError(requestId, error));
        }

        void StartInterruptTimeout(int requestId)
        {
            StopInterruptTimeout();
            interruptTimeoutRoutine = StartCoroutine(InterruptTimeoutCoroutine(requestId));
        }

        void StopInterruptTimeout()
        {
            if (interruptTimeoutRoutine == null)
                return;

            StopCoroutine(interruptTimeoutRoutine);
            interruptTimeoutRoutine = null;
        }

        IEnumerator InterruptTimeoutCoroutine(int requestId)
        {
            yield return new WaitForSecondsRealtime(interruptAiTimeoutSeconds);

            if (requestId != activeRequestId || !waitingForAi)
                yield break;

            HandleInterruptError(requestId, "Interrupt AI request timed out.");
        }

        void HandleInterruptSuccess(int requestId, string response)
        {
            if (requestId != activeRequestId)
                return;

            StopInterruptTimeout();
            ShowInterruptLine(response);
        }

        void HandleInterruptError(int requestId, string error)
        {
            if (requestId != activeRequestId)
                return;

            StopInterruptTimeout();
            Debug.LogWarning("[RoamingNpcInterrupter] " + error);
            ShowInterruptLine(config != null ? config.GetRandomFallbackLine() : "嘿，等等我！");
        }

        void ShowInterruptLine(string line)
        {
            waitingForAi = false;

            if (ShouldYieldToScriptOrAiChat())
            {
                brain?.SetState(RoamingNpcState.Roaming);
                controller?.WakeUpWander();
                return;
            }

            if (config == null)
            {
                brain?.SetState(RoamingNpcState.Roaming);
                controller?.WakeUpWander();
                return;
            }

            if (interruptDialogueUI == null)
            {
                Debug.LogWarning("[RoamingNpcInterrupter] InterruptDialogueUI is missing.");
                brain?.SetState(RoamingNpcState.Roaming);
                controller?.WakeUpWander();
                return;
            }

            var text = string.IsNullOrWhiteSpace(line) ? config.GetRandomFallbackLine() : line;
            interruptDialogueUI.Show(
                config.DisplayName,
                config.Portrait,
                text,
                config.InterruptTypewriterCps);
            brain.SetState(RoamingNpcState.InterruptShowing);
        }

        bool TryGetPlayerPlanarPosition(out Vector3 playerPosition)
        {
            playerPosition = default;

            if (playerTransform == null)
            {
                var inventory = FindFirstObjectByType<PlayerInventory>();
                if (inventory == null)
                    return false;

                playerTransform = inventory.transform;
            }

            playerPosition = playerTransform.position;
            return true;
        }

        static float GetPlanarDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        public static bool ShouldYieldToScriptOrAiChat()
        {
            if (TwoWorlds.Dialogue.DialogueManager.Instance != null &&
                TwoWorlds.Dialogue.DialogueManager.Instance.IsPlaying)
                return true;

            var chatSession = AIChatSession.FindInstance();
            if (chatSession != null && chatSession.IsActive)
                return true;

            var readinessSession = EnterYinReadinessSession.FindInstance();
            return readinessSession != null && readinessSession.IsActive;
        }

        void AbortInterruptForExternalDialogue()
        {
            if (waitingForAi)
            {
                waitingForAi = false;
                activeRequestId++;
                StopInterruptTimeout();
            }

            if (interruptDialogueUI != null && interruptDialogueUI.IsShowing)
            {
                DismissInterrupt();
                return;
            }

            if (brain != null && brain.State == RoamingNpcState.Approaching)
                CancelApproach(0f);
        }
    }
}

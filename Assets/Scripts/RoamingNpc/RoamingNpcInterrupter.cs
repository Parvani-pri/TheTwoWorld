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

        Transform playerTransform;
        float cooldownUntil;
        bool interruptLogicEnabled = true;
        int activeRequestId;
        bool waitingForAi;

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

            if (!interruptLogicEnabled || brain == null)
                return;

            if (brain.State == RoamingNpcState.InterruptShowing || waitingForAi)
                return;

            if (brain.State == RoamingNpcState.Approaching)
            {
                UpdateApproaching();
                return;
            }

            if (!brain.AllowInterruptLogic())
                return;

            if (Time.time < cooldownUntil)
                return;

            if (!TryGetPlayerPlanarPosition(out var playerPosition))
                return;

            if (GetPlanarDistance(transform.position, playerPosition) > config.SenseRadius)
                return;

            BeginApproach(playerPosition);
        }

        public void SetInterruptEnabled(bool enabled)
        {
            interruptLogicEnabled = enabled;
        }

        public void DismissInterrupt()
        {
            waitingForAi = false;
            activeRequestId++;

            interruptDialogueUI?.Hide();

            if (config != null)
                cooldownUntil = Time.time + config.InterruptCooldownSeconds;

            brain?.SetState(RoamingNpcState.Cooldown);
            controller?.StopMoving();
        }

        void UpdateUnlockState()
        {
            if (brain == null || gameProgress == null)
                return;

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
                brain.SetState(RoamingNpcState.Roaming);
        }

        void BeginApproach(Vector3 playerPosition)
        {
            if (config == null || controller == null || brain == null)
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

            if (!controller.HasReachedTarget())
                return;

            controller.StopMoving();
            RequestInterruptLine();
        }

        void CancelApproach(float cooldownSeconds)
        {
            controller?.StopMoving();
            cooldownUntil = Time.time + cooldownSeconds;
            brain?.SetState(RoamingNpcState.Roaming);
        }

        void RequestInterruptLine()
        {
            if (config == null || brain == null)
                return;

            brain.SetState(RoamingNpcState.InterruptShowing);
            waitingForAi = true;

            if (aiService == null)
            {
                ShowInterruptLine(config.GetRandomFallbackLine());
                return;
            }

            var inventory = playerTransform != null
                ? playerTransform.GetComponent<PlayerInventory>()
                : FindFirstObjectByType<PlayerInventory>();

            var requestId = ++activeRequestId;
            aiService.AskInterruptLine(
                inventory,
                config.InterruptPersona,
                response => HandleInterruptSuccess(requestId, response),
                error => HandleInterruptError(requestId, error));
        }

        void HandleInterruptSuccess(int requestId, string response)
        {
            if (requestId != activeRequestId)
                return;

            ShowInterruptLine(response);
        }

        void HandleInterruptError(int requestId, string error)
        {
            if (requestId != activeRequestId)
                return;

            Debug.LogWarning("[RoamingNpcInterrupter] " + error);
            ShowInterruptLine(config != null ? config.GetRandomFallbackLine() : "嘿，等等我！");
        }

        void ShowInterruptLine(string line)
        {
            waitingForAi = false;

            if (interruptDialogueUI == null || config == null)
                return;

            var text = string.IsNullOrWhiteSpace(line) ? config.GetRandomFallbackLine() : line;
            interruptDialogueUI.Show(
                config.DisplayName,
                config.Portrait,
                text,
                config.InterruptTypewriterCps);
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
    }
}

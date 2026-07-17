using UnityEngine;

namespace TwoWorlds.RoamingNpc
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class RoamingNpcController : MonoBehaviour
    {
        [SerializeField] RoamingNpcConfig config;
        [SerializeField] RoamingNpcBrain brain;
        [SerializeField] Transform wanderAnchor;
        [SerializeField] Animator animator;
        [SerializeField] string speedParameter = "speed";
        [SerializeField] float idleSpeedValue;
        [SerializeField] float wanderSpeedValue = 0.5f;
        [SerializeField] float approachSpeedValue = 1f;

        static readonly int SpeedHash = Animator.StringToHash("speed");

        Vector3 anchorPosition;
        Vector3 moveTarget;
        float nextWanderDecisionTime;
        bool movementEnabled = true;
        bool scriptedOverride;
        bool hasMoveTarget;
        SpriteRenderer spriteRenderer;

        public bool IsMovingToTarget { get; private set; }
        public bool IsScriptedOverride => scriptedOverride;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (brain == null)
                brain = GetComponent<RoamingNpcBrain>();

            if (animator == null)
                animator = GetComponent<Animator>();

            if (wanderAnchor == null)
                wanderAnchor = transform;

            anchorPosition = wanderAnchor.position;
            ScheduleWanderIdle();
        }

        public void SetMovementEnabled(bool enabled)
        {
            movementEnabled = enabled;

            if (!enabled && !scriptedOverride)
            {
                IsMovingToTarget = false;
                UpdateAnimatorSpeed();
            }
        }

        public void SetScriptedOverride(bool enabled)
        {
            scriptedOverride = enabled;

            if (!enabled && !movementEnabled)
                IsMovingToTarget = false;
        }

        public void SetAnchorToCurrentPosition()
        {
            anchorPosition = wanderAnchor.position;
        }

        public void PickWanderTarget()
        {
            if (config == null)
                return;

            var offset = Random.insideUnitCircle * config.WanderRadius;
            moveTarget = anchorPosition + new Vector3(offset.x, 0f, offset.y);
            moveTarget.y = transform.position.y;
            hasMoveTarget = true;
            IsMovingToTarget = true;
        }

        public void MoveTo(Vector3 worldTarget)
        {
            moveTarget = worldTarget;
            moveTarget.y = transform.position.y;
            hasMoveTarget = true;
            IsMovingToTarget = true;
        }

        public bool HasReachedTarget()
        {
            if (!hasMoveTarget || config == null)
                return false;

            return GetPlanarDistance(transform.position, moveTarget) <= config.ArriveThreshold;
        }

        public void StopMoving()
        {
            hasMoveTarget = false;
            IsMovingToTarget = false;
            UpdateAnimatorSpeed();
        }

        public void WakeUpWander()
        {
            nextWanderDecisionTime = Time.time;

            if (brain == null ||
                (brain.State != RoamingNpcState.Roaming && brain.State != RoamingNpcState.Cooldown) ||
                hasMoveTarget)
            {
                return;
            }

            TryBeginWanderMove();
        }

        void Update()
        {
            if (config == null)
                return;

            var allowByBrain = brain == null ||
                               brain.AllowRoamingMovement() ||
                               brain.State == RoamingNpcState.Approaching ||
                               brain.IsScriptedBeatActive;

            if (!scriptedOverride && (!movementEnabled || !allowByBrain))
            {
                UpdateAnimatorSpeed();
                return;
            }

            if (brain != null &&
                !scriptedOverride &&
                (brain.State == RoamingNpcState.Roaming || brain.State == RoamingNpcState.Cooldown) &&
                Time.time >= nextWanderDecisionTime &&
                !hasMoveTarget)
            {
                TryBeginWanderMove();
            }

            if (!hasMoveTarget)
            {
                UpdateAnimatorSpeed();
                return;
            }

            var current = transform.position;
            var step = GetCurrentMoveSpeed() * Time.deltaTime;
            var next = Vector3.MoveTowards(current, moveTarget, step);
            next.y = current.y;
            transform.position = next;

            var planarDeltaX = next.x - current.x;
            FacePlanarDirection(planarDeltaX != 0f ? planarDeltaX : moveTarget.x - current.x);

            if (GetPlanarDistance(next, moveTarget) <= config.ArriveThreshold)
            {
                hasMoveTarget = false;
                IsMovingToTarget = false;

                if (brain != null &&
                    !scriptedOverride &&
                    (brain.State == RoamingNpcState.Roaming || brain.State == RoamingNpcState.Cooldown))
                {
                    ScheduleWanderIdle();
                }
            }

            UpdateAnimatorSpeed();
        }

        void UpdateAnimatorSpeed()
        {
            if (animator == null)
                return;

            var speed = idleSpeedValue;

            if (hasMoveTarget || IsMovingToTarget)
            {
                speed = brain != null && brain.State == RoamingNpcState.Approaching
                    ? approachSpeedValue
                    : wanderSpeedValue;
            }

            var parameterHash = string.IsNullOrWhiteSpace(speedParameter)
                ? SpeedHash
                : Animator.StringToHash(speedParameter);

            if (!AnimatorHasFloatParameter(animator, parameterHash))
                return;

            animator.SetFloat(parameterHash, speed);
        }

        static bool AnimatorHasFloatParameter(Animator targetAnimator, int parameterHash)
        {
            foreach (var parameter in targetAnimator.parameters)
            {
                if (parameter.nameHash == parameterHash &&
                    parameter.type == AnimatorControllerParameterType.Float)
                {
                    return true;
                }
            }

            return false;
        }

        void TryBeginWanderMove()
        {
            if (config == null)
                return;

            if (Random.value <= config.WanderMoveChance)
            {
                PickWanderTarget();
                return;
            }

            ScheduleWanderIdle();
        }

        void ScheduleWanderIdle()
        {
            if (config == null)
                return;

            var idleDuration = Random.Range(config.WanderIdleDurationMin, config.WanderIdleDurationMax);
            nextWanderDecisionTime = Time.time + idleDuration;
        }

        float GetCurrentMoveSpeed()
        {
            if (config == null)
                return 0f;

            return brain != null && brain.State == RoamingNpcState.Approaching
                ? config.MoveSpeed
                : config.WanderMoveSpeed;
        }

        void FacePlanarDirection(float horizontal)
        {
            if (Mathf.Approximately(horizontal, 0f))
                return;

            var scale = transform.localScale;
            scale.x = horizontal > 0f ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;

            if (spriteRenderer != null)
                spriteRenderer.flipX = false;
        }

        static float GetPlanarDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}

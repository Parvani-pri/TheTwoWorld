using UnityEngine;

namespace TwoWorlds.RoamingNpc
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class RoamingNpcController : MonoBehaviour
    {
        [SerializeField] RoamingNpcConfig config;
        [SerializeField] RoamingNpcBrain brain;
        [SerializeField] Transform wanderAnchor;

        Vector3 anchorPosition;
        Vector3 moveTarget;
        float nextPickTime;
        bool movementEnabled = true;
        bool hasMoveTarget;
        SpriteRenderer spriteRenderer;

        public bool IsMovingToTarget { get; private set; }

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (brain == null)
                brain = GetComponent<RoamingNpcBrain>();

            if (wanderAnchor == null)
                wanderAnchor = transform;

            anchorPosition = wanderAnchor.position;
            ScheduleNextWanderTarget();
        }

        public void SetMovementEnabled(bool enabled)
        {
            movementEnabled = enabled;

            if (!enabled)
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
            ScheduleNextWanderTarget();
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
        }

        public void WakeUpWander()
        {
            nextPickTime = Time.time;

            if (brain == null ||
                (brain.State != RoamingNpcState.Roaming && brain.State != RoamingNpcState.Cooldown) ||
                hasMoveTarget)
            {
                return;
            }

            PickWanderTarget();
        }

        void Update()
        {
            if (!movementEnabled || config == null)
                return;

            if (brain != null && !brain.AllowRoamingMovement() && brain.State != RoamingNpcState.Approaching)
                return;

            if (brain != null &&
                (brain.State == RoamingNpcState.Roaming || brain.State == RoamingNpcState.Cooldown) &&
                Time.time >= nextPickTime &&
                !hasMoveTarget)
            {
                PickWanderTarget();
            }

            if (!hasMoveTarget)
                return;

            var current = transform.position;
            var step = config.MoveSpeed * Time.deltaTime;
            var next = Vector3.MoveTowards(current, moveTarget, step);
            next.y = current.y;
            transform.position = next;

            FacePlanarDirection(moveTarget.x - current.x);

            if (GetPlanarDistance(next, moveTarget) <= config.ArriveThreshold)
            {
                hasMoveTarget = false;
                IsMovingToTarget = false;
            }
        }

        void ScheduleNextWanderTarget()
        {
            if (config == null)
                return;

            var delay = Random.Range(config.PickTargetIntervalMin, config.PickTargetIntervalMax);
            nextPickTime = Time.time + delay;
        }

        void FacePlanarDirection(float deltaX)
        {
            if (spriteRenderer == null || Mathf.Abs(deltaX) < 0.01f)
                return;

            spriteRenderer.flipX = deltaX < 0f;
        }

        static float GetPlanarDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}

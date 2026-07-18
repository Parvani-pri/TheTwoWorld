using TwoWorlds.Combat;
using TwoWorlds.RoamingNpc;
using UnityEngine;

namespace TwoWorlds.Dialogue
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class DialogueActorController : MonoBehaviour
    {
        [SerializeField] string actorKey;
        [SerializeField] float moveSpeed = 2f;
        [SerializeField] float arriveThreshold = 0.25f;
        [SerializeField] RoamingNpcController roamingController;
        [SerializeField] RoamingNpcBrain roamingBrain;

        Vector3 moveTarget;
        bool hasMoveTarget;
        bool usingScriptedOverride;
        PendingFacing pendingFacing;
        SpriteRenderer spriteRenderer;
        CombatActor combatActor;

        enum PendingFacing
        {
            None,
            Left,
            Right
        }

        public string ActorKey => string.IsNullOrWhiteSpace(actorKey) ? name : actorKey.Trim();
        public bool IsMoving => hasMoveTarget || (roamingController != null && roamingController.IsMovingToTarget);

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (roamingController == null)
                roamingController = GetComponent<RoamingNpcController>();

            if (roamingBrain == null)
                roamingBrain = GetComponent<RoamingNpcBrain>();

            combatActor = GetComponent<CombatActor>();
        }

        void OnEnable()
        {
            DialogueActorRegistry.Register(this);
        }

        void OnDisable()
        {
            DialogueActorRegistry.Unregister(this);
            ClearScriptedOverride();
        }

        public void MoveTo(Vector3 worldTarget)
        {
            EnableScriptedOverride();

            if (roamingController != null)
            {
                roamingController.MoveTo(worldTarget);
                return;
            }

            moveTarget = worldTarget;
            moveTarget.y = transform.position.y;
            hasMoveTarget = true;
        }

        public void FacePlanarTarget(Vector3 worldTarget)
        {
            var horizontal = worldTarget.x - transform.position.x;
            if (Mathf.Approximately(horizontal, 0f))
                return;

            SetFacingSign(horizontal > 0f ? 1f : -1f);
        }

        public void FaceLeft()
        {
            SetFacingSign(-1f);
            pendingFacing = PendingFacing.None;
        }

        public void FaceRight()
        {
            SetFacingSign(1f);
            pendingFacing = PendingFacing.None;
        }

        public void QueueFaceLeft()
        {
            if (IsMoving)
                pendingFacing = PendingFacing.Left;
            else
                FaceLeft();
        }

        public void QueueFaceRight()
        {
            if (IsMoving)
                pendingFacing = PendingFacing.Right;
            else
                FaceRight();
        }

        void SetFacingSign(float sign)
        {
            var scale = transform.localScale;
            scale.x = sign >= 0f ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;

            if (spriteRenderer != null)
                spriteRenderer.flipX = false;
        }

        public void StopMoving()
        {
            hasMoveTarget = false;

            if (roamingController != null)
                roamingController.StopMoving();
        }

        public void ClearScriptedOverride()
        {
            if (!usingScriptedOverride)
                return;

            usingScriptedOverride = false;
            pendingFacing = PendingFacing.None;

            if (roamingController != null)
                roamingController.SetScriptedOverride(false);

            if (roamingBrain != null)
                roamingBrain.EndScriptedBeat();

            SyncCombatGroundFromTransform();
        }

        void Update()
        {
            if (roamingController == null && hasMoveTarget)
            {
                var current = transform.position;
                var next = Vector3.MoveTowards(current, moveTarget, moveSpeed * Time.deltaTime);
                next.y = current.y;
                transform.position = next;
                SyncCombatGroundFromTransform();
                FacePlanarTarget(moveTarget);

                if (GetPlanarDistance(next, moveTarget) <= arriveThreshold)
                    hasMoveTarget = false;
            }

            TryApplyPendingFacing();
        }

        void TryApplyPendingFacing()
        {
            if (pendingFacing == PendingFacing.None || IsMoving)
                return;

            switch (pendingFacing)
            {
                case PendingFacing.Left:
                    FaceLeft();
                    break;
                case PendingFacing.Right:
                    FaceRight();
                    break;
            }
        }

        void EnableScriptedOverride()
        {
            if (usingScriptedOverride)
                return;

            var interrupter = GetComponent<RoamingNpcInterrupter>();
            interrupter?.SuspendForScriptedBeat();

            usingScriptedOverride = true;

            if (roamingController != null)
                roamingController.SetScriptedOverride(true);

            if (roamingBrain != null)
                roamingBrain.BeginScriptedBeat();
        }

        void SyncCombatGroundFromTransform()
        {
            combatActor?.SyncGroundFromTransform();
        }

        static float GetPlanarDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}

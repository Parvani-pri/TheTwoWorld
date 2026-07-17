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
        SpriteRenderer spriteRenderer;

        public string ActorKey => string.IsNullOrWhiteSpace(actorKey) ? name : actorKey.Trim();
        public bool IsMoving => hasMoveTarget || (roamingController != null && roamingController.IsMovingToTarget);

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (roamingController == null)
                roamingController = GetComponent<RoamingNpcController>();

            if (roamingBrain == null)
                roamingBrain = GetComponent<RoamingNpcBrain>();
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
            var deltaX = worldTarget.x - transform.position.x;
            if (spriteRenderer == null || Mathf.Abs(deltaX) < 0.01f)
                return;

            spriteRenderer.flipX = deltaX < 0f;
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

            if (roamingController != null)
                roamingController.SetScriptedOverride(false);

            if (roamingBrain != null)
                roamingBrain.EndScriptedBeat();
        }

        void Update()
        {
            if (roamingController != null || !hasMoveTarget)
                return;

            var current = transform.position;
            var next = Vector3.MoveTowards(current, moveTarget, moveSpeed * Time.deltaTime);
            next.y = current.y;
            transform.position = next;
            FacePlanarTarget(moveTarget);

            if (GetPlanarDistance(next, moveTarget) <= arriveThreshold)
                hasMoveTarget = false;
        }

        void EnableScriptedOverride()
        {
            if (usingScriptedOverride)
                return;

            usingScriptedOverride = true;

            if (roamingController != null)
                roamingController.SetScriptedOverride(true);

            if (roamingBrain != null)
                roamingBrain.BeginScriptedBeat();
        }

        static float GetPlanarDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}

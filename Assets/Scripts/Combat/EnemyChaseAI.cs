using UnityEngine;

namespace TwoWorlds.Combat
{
    /// <summary>
    /// Moves toward the closest player actor on the stage. Follows the target
    /// into the air when it flies, and lands once the target is grounded.
    /// </summary>
    [RequireComponent(typeof(CombatActor))]
    public class EnemyChaseAI : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 3f;
        [SerializeField] float verticalSpeed = 2f;
        [Tooltip("Stops approaching once the ground distance to the target is below this.")]
        [SerializeField] float stopDistance = 1.2f;
        [SerializeField] bool canFly = true;
        [Tooltip("Height difference below which the enemy stops adjusting altitude.")]
        [SerializeField] float heightTolerance = 0.15f;
        [SerializeField] Animator animator;

        
        SpriteRenderer spriteRenderer;

        CombatActor actor;
        IEnemyAttackRangeProvider attackAI;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            actor = GetComponent<CombatActor>();
            attackAI = GetComponent<IEnemyAttackRangeProvider>();
        }

        void Update()
        {
            if (attackAI != null && attackAI.IsInAttackRange)
                return;

            var target = CombatActor.FindClosest(CombatFaction.Player, actor.GroundPosition);
            if (target == null)
                return;
            ChaseGround(target);


            if (canFly)
                ChaseHeight(target);
        }

        void ChaseGround(CombatActor target)
        {
            var toTarget = target.GroundPosition - actor.GroundPosition;
            if (toTarget.magnitude <= stopDistance + 0.02f && !GetComponent<EnemyAttackAI>().isPlayerDead)
            {
                animator.SetTrigger(PlayerAnimParams.ATTACK);
                animator.SetBool(PlayerAnimParams.IS_WALK, false);
                return;
            }
            if (!GetComponent<EnemyAttackAI>().isPlayerDead)
            {
                animator.SetBool(PlayerAnimParams.IS_WALK, true);
                var step = toTarget.normalized * moveSpeed * Time.deltaTime;
                actor.MoveGround(step);
                FaceTarget(toTarget.x);
            }

        }

        void ChaseHeight(CombatActor target)
        {
            var heightDiff = target.Height - actor.Height;
            if (Mathf.Abs(heightDiff) <= heightTolerance)
                return;

            var step = Mathf.Sign(heightDiff) * verticalSpeed * Time.deltaTime;
            actor.MoveHeight(step);
        }

        void FaceTarget(float horizontal)
        {
            spriteRenderer.flipX = horizontal <= 0f;
            if (Mathf.Approximately(horizontal, 0f))
                return;

        }

        public void SetStopDistance(float stopDistance)
        {
            this.stopDistance = stopDistance;
        }
    }
}

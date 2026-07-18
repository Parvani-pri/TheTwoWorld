using TwoWorlds.Core;
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
        [SerializeField] Collider2D col;
        
        SpriteRenderer spriteRenderer;


        CombatActor actor;
        IEnemyAttackRangeProvider attackAI;
        bool gameplayBlocked;
        bool hasTriggeredOnDie = false;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            actor = GetComponent<CombatActor>();
            attackAI = GetComponent<IEnemyAttackRangeProvider>();
        }

        void OnEnable() => GameEvents.GameplayInputBlocked += OnGameplayInputBlocked;

        void OnDisable() => GameEvents.GameplayInputBlocked -= OnGameplayInputBlocked;

        void Update()
        {
            if (gameplayBlocked)
            {
                if (animator != null)
                    animator.SetBool(PlayerAnimParams.IS_WALK, false);

                return;
            }

            if (attackAI != null && attackAI.IsInAttackRange)
                return;

            var target = CombatActor.FindClosest(CombatFaction.Player, actor.GroundPosition);
            if (target == null)
                return;
            if (!hasTriggeredOnDie)
            {
                ChaseGround(target);
            }



            if (canFly)
                ChaseHeight(target);
        }

        void ChaseGround(CombatActor target)
        {
            if (GetComponent<CombatHealth>().IsDead)
            {
                animator.SetTrigger(PlayerAnimParams.ON_DIE);
                animator.SetBool(PlayerAnimParams.IS_WALK, false);
                hasTriggeredOnDie = true;
                return;
            }
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
            transform.localScale = new Vector3(horizontal <= 0f ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x),
                transform.localScale.y, transform.localScale.z);

            if (Mathf.Approximately(horizontal, 0f))
                return;

        }

        public void SetStopDistance(float stopDistance)
        {
            this.stopDistance = stopDistance;
        }

        void OnGameplayInputBlocked(bool blocked) => gameplayBlocked = blocked;
    }
}

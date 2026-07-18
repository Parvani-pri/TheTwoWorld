using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Combat
{
    [RequireComponent(typeof(CombatActor))]
    public class EnemyAttackAI : MonoBehaviour, IEnemyAttackRangeProvider
    {
        [SerializeField] AttackData attack1Data;
        [SerializeField] AttackData attack2Data;
        [SerializeField] AttackData attack3Data;
        [SerializeField] CombatHitbox hitbox;
        [SerializeField] EnemyChaseAI chaseAI;
        [SerializeField] Animator animator;
        [SerializeField] float attackCD = 3f;

        bool canAttack = true;
        bool gameplayBlocked;
        public bool isPlayerDead = false;
        bool shouldCountdown = false;
        CombatActor actor;
        CombatActor target;
        float cooldownTimer;
        int nextAttackID;

        public bool IsInAttackRange { get; private set; }

        void Awake()
        {
            nextAttackID = Random.Range(1, 4);
            switch (nextAttackID)
            {
                case 1:
                    chaseAI.SetStopDistance(attack1Data.AttackRange);
                    break;
                case 2:
                    chaseAI.SetStopDistance(attack2Data.AttackRange);
                    break;
                case 3:
                    chaseAI.SetStopDistance(attack3Data.AttackRange);
                    break;

            }
            animator.SetInteger(PlayerAnimParams.ATTACK_INDEX, nextAttackID);
            actor = GetComponent<CombatActor>();

            if (hitbox == null)
                hitbox = GetComponentInChildren<CombatHitbox>();
        }

        void OnEnable() => GameEvents.GameplayInputBlocked += OnGameplayInputBlocked;

        void OnDisable() => GameEvents.GameplayInputBlocked -= OnGameplayInputBlocked;

        void Update()
        {
            if (gameplayBlocked)
                return;

            target = CombatActor.FindClosest(CombatFaction.Player, actor.GroundPosition);
            //IsInAttackRange = false;

            if (target == null || attack1Data == null || attack2Data == null || attack3Data == null || hitbox == null)
                return;

            var groundDistance = Vector2.Distance(actor.GroundPosition, target.GroundPosition);
            var heightDiff = Mathf.Abs(actor.Height - target.Height);

            switch (nextAttackID)
            {
                case 1:
                    IsInAttackRange = groundDistance <= attack1Data.AttackRange
                    && heightDiff <= attack1Data.HeightTolerance;
                    break;
                case 2:
                    IsInAttackRange = groundDistance <= attack2Data.AttackRange
                    && heightDiff <= attack1Data.HeightTolerance;
                    break;
                case 3:
                    IsInAttackRange = groundDistance <= attack3Data.AttackRange
                    && heightDiff <= attack1Data.HeightTolerance;
                    break;

            }

            if (cooldownTimer > 0f && shouldCountdown && !isPlayerDead)
            {
                cooldownTimer -= Time.deltaTime;
                return;
            }
            if (cooldownTimer <= 0f)
            {
                shouldCountdown = false;
                canAttack = true;
            }

            if (!IsInAttackRange || hitbox.IsActive)
                return;

            FaceTarget(target);
            if (canAttack && cooldownTimer <= 0f && !isPlayerDead)
            {
                cooldownTimer = attackCD;
                switch (nextAttackID)
                {
                    case 1:
                        animator.SetTrigger(PlayerAnimParams.ATTACK);
                        animator.SetInteger(PlayerAnimParams.ATTACK_INDEX, nextAttackID);
                        break;
                    case 2:
                        animator.SetTrigger(PlayerAnimParams.ATTACK);
                        animator.SetInteger(PlayerAnimParams.ATTACK_INDEX, nextAttackID);
                        break;
                    case 3:
                        animator.SetTrigger(PlayerAnimParams.ATTACK);
                        animator.SetInteger(PlayerAnimParams.ATTACK_INDEX, nextAttackID);
                        break;
                }
                canAttack = false;
                nextAttackID = Random.Range(1, 4);
                animator.SetInteger(PlayerAnimParams.ATTACK_INDEX, nextAttackID);
                switch (nextAttackID)
                {
                    case 1:
                        chaseAI.SetStopDistance(attack1Data.AttackRange);
                        break;
                    case 2:
                        chaseAI.SetStopDistance(attack2Data.AttackRange);
                        break;
                    case 3:
                        chaseAI.SetStopDistance(attack3Data.AttackRange);
                        break;

                }
            }
            else if (isPlayerDead)
            {
                animator.ResetTrigger(PlayerAnimParams.ATTACK);
                animator.SetInteger(PlayerAnimParams.ATTACK_INDEX, -1);
            }
        }

        void FaceTarget(CombatActor chaseTarget)
        {
            //var horizontal = chaseTarget.GroundPosition.x - actor.GroundPosition.x;
            //if (Mathf.Approximately(horizontal, 0f))
            //    return;

            //var scale = hitbox.transform.localScale;
            //scale.x = horizontal > 0f ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            //hitbox.transform.localScale = scale;
        }

        public void LaunchAttack(int attackID)
        {
            switch (attackID)
            {
                case 1:
                    hitbox.Activate(attack1Data, 1);
                    break;
                case 2:
                    hitbox.Activate(attack2Data, 1);
                    break;
                case 3:
                    hitbox.Activate(attack3Data, 1);
                    break;
            }
        }

        public void StartCooldown()
        {
            animator.ResetTrigger(PlayerAnimParams.ATTACK);
            shouldCountdown = true;
        }

        public void IsPlayerDead(bool isDead)
        {
            animator.SetBool(PlayerAnimParams.IS_WALK, false);
            isPlayerDead = isDead;
        }

        void OnGameplayInputBlocked(bool blocked)
        {
            gameplayBlocked = blocked;

            if (!blocked || animator == null)
                return;

            animator.ResetTrigger(PlayerAnimParams.ATTACK);
            animator.SetInteger(PlayerAnimParams.ATTACK_INDEX, -1);
            hitbox?.Deactivate();
        }

    }
}

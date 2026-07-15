using UnityEngine;

namespace TwoWorlds.Combat
{
    [RequireComponent(typeof(CombatActor))]
    public class EnemyAttackAI : MonoBehaviour, IEnemyAttackRangeProvider
    {
        [SerializeField] AttackData attackData;
        [SerializeField] CombatHitbox hitbox;

        CombatActor actor;
        CombatActor target;
        float cooldownTimer;

        public bool IsInAttackRange { get; private set; }

        void Awake()
        {
            actor = GetComponent<CombatActor>();

            if (hitbox == null)
                hitbox = GetComponentInChildren<CombatHitbox>();
        }

        void Update()
        {
            target = CombatActor.FindClosest(CombatFaction.Player, actor.GroundPosition);
            IsInAttackRange = false;

            if (target == null || attackData == null || hitbox == null)
                return;

            var groundDistance = Vector2.Distance(actor.GroundPosition, target.GroundPosition);
            var heightDiff = Mathf.Abs(actor.Height - target.Height);

            IsInAttackRange = groundDistance <= attackData.AttackRange
                && heightDiff <= attackData.HeightTolerance;

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
                return;
            }

            if (!IsInAttackRange || hitbox.IsActive)
                return;

            FaceTarget(target);
            hitbox.Activate(attackData);
            cooldownTimer = attackData.Cooldown;
        }

        void FaceTarget(CombatActor chaseTarget)
        {
            var horizontal = chaseTarget.GroundPosition.x - actor.GroundPosition.x;
            if (Mathf.Approximately(horizontal, 0f))
                return;

            var scale = hitbox.transform.localScale;
            scale.x = horizontal > 0f ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            hitbox.transform.localScale = scale;
        }
    }
}

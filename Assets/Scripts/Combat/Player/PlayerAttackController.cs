using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Combat
{
    public class PlayerAttackController : MonoBehaviour, IPlayerAttackState
    {
        [SerializeField] InputReader inputReader;
        [SerializeField] AttackData attackData;
        [SerializeField] CombatHitbox hitbox;
        [SerializeField] Animator playerAnimator;


        float cooldownTimer;
        public bool canAttack;
        bool combatEnded;
        bool gameplayBlocked;
        bool inventoryOpen;

        public bool IsAttacking => hitbox != null && hitbox.IsActive;

        void Awake()
        {
            canAttack = true;
            if (hitbox == null)
                hitbox = GetComponentInChildren<CombatHitbox>();
        }

        void OnEnable()
        {
            GameEvents.CombatEnded += OnCombatEnded;
            GameEvents.GameplayInputBlocked += OnGameplayInputBlocked;
            GameEvents.InventoryOpenChanged += OnInventoryOpenChanged;
        }

        void OnDisable()
        {
            GameEvents.CombatEnded -= OnCombatEnded;
            GameEvents.GameplayInputBlocked -= OnGameplayInputBlocked;
            GameEvents.InventoryOpenChanged -= OnInventoryOpenChanged;
        }

        void Start()
        {
            if (inputReader == null)
                inputReader = FindFirstObjectByType<InputReader>();

            if (inputReader?.AttackAction1 == null)
                Debug.LogWarning("[PlayerAttackController] Attack action not found on InputReader.");
        }

        void Update()
        {
            if (cooldownTimer > 0f)
                cooldownTimer -= Time.deltaTime;

            if (inputReader?.AttackAction1 != null && inputReader.AttackAction1.WasPerformedThisFrame() && cooldownTimer <= 0f &&  canAttack)
            {
                playerAnimator.SetTrigger(PlayerAnimParams.ATTACK);
                playerAnimator.SetInteger(PlayerAnimParams.ATTACK_INDEX, 1);
            }

            else if (inputReader?.AttackAction2 != null && inputReader.AttackAction2.WasPerformedThisFrame() && cooldownTimer <= 0f && canAttack)
            {
                playerAnimator.SetTrigger(PlayerAnimParams.ATTACK);
                playerAnimator.SetInteger(PlayerAnimParams.ATTACK_INDEX, 2);
            }

            else if (inputReader?.AttackAction3 != null && inputReader.AttackAction3.WasPerformedThisFrame() && cooldownTimer <= 0f && canAttack)
            {
                playerAnimator.SetTrigger(PlayerAnimParams.ATTACK);
                playerAnimator.SetInteger(PlayerAnimParams.ATTACK_INDEX, 3);
            }

        }

        public void TryAttack()
        {
            if (IsInputBlocked() || attackData == null || hitbox == null)
                return;

            if (cooldownTimer > 0f || hitbox.IsActive)
                return;

            hitbox.Activate(attackData);
            cooldownTimer = attackData.Cooldown;
        }

        void OnCombatEnded(CombatResult _) => combatEnded = true;

        void OnGameplayInputBlocked(bool blocked)
        {
            gameplayBlocked = blocked;
            if (blocked)
                hitbox?.Deactivate();
        }

        void OnInventoryOpenChanged(bool open)
        {
            inventoryOpen = open;
            if (open)
                hitbox?.Deactivate();
        }

        bool IsInputBlocked()
        {
            return combatEnded
                || gameplayBlocked
                || inventoryOpen
                || attackData == null;
        }

        public void SetAttackEligibility(int eligibility)
        {
            canAttack = eligibility != 0;
        }
    }
}

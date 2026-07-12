using TwoWorlds.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TwoWorlds.Combat
{
    public class PlayerAttackController : MonoBehaviour, IPlayerAttackState
    {
        [SerializeField] InputReader inputReader;
        [SerializeField] AttackData attackData;
        [SerializeField] CombatHitbox hitbox;

        float cooldownTimer;
        bool combatEnded;
        bool gameplayBlocked;
        bool inventoryOpen;

        public bool IsAttacking => hitbox != null && hitbox.IsActive;

        void Awake()
        {
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

            if (inputReader?.AttackAction == null)
                Debug.LogWarning("[PlayerAttackController] Attack action not found on InputReader.");
        }

        void Update()
        {
            if (cooldownTimer > 0f)
                cooldownTimer -= Time.deltaTime;

            if (inputReader?.AttackAction != null && inputReader.AttackAction.WasPerformedThisFrame())
                TryAttack();
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
    }
}

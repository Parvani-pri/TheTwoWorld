using System.Collections;
using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Combat
{
    public class PlayerAttackController : MonoBehaviour, IPlayerAttackState
    {
        [SerializeField] InputReader inputReader;
        [SerializeField] AttackData attack1Data;
        [SerializeField] AttackData attack2Data;
        [SerializeField] AttackData attack3Data;
        [SerializeField] CombatHitbox hitbox;
        [SerializeField] Animator playerAnimator;


        [SerializeField] GameObject chargeProjectilePrefab;
        GameObject chargeProjectileInstance;
        [SerializeField] Transform spawnTransform;
        [SerializeField] float maxChargeTimer = 3f;
        [SerializeField] float maxChargeAttackMultiplier = 3f;
        float currentChargeTimer;

        float cooldownTimer1;
        float cooldownTimer2;
        float cooldownTimer3;
        bool canAttack;
        bool combatEnded;
        bool gameplayBlocked;
        bool inventoryOpen;

        public bool IsAttacking => hitbox != null && hitbox.IsActive;

        void Awake()
        {
            canAttack = true;
            currentChargeTimer = 0f;
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
            if (cooldownTimer1 > 0f)
                cooldownTimer1 -= Time.deltaTime;
            if (cooldownTimer2 > 0f)
                cooldownTimer2 -= Time.deltaTime;
            if (cooldownTimer3 > 0f)
                cooldownTimer3 -= Time.deltaTime;

            if (IsInputBlocked())
                return;

            if (inputReader?.AttackAction1 != null && inputReader.AttackAction1.WasPerformedThisFrame() && cooldownTimer1 <= 0f &&  canAttack)
            {

                playerAnimator.SetTrigger(PlayerAnimParams.ATTACK);
                playerAnimator.SetInteger(PlayerAnimParams.ATTACK_INDEX, 1);
            }

            if (inputReader?.AttackAction2 != null && inputReader.AttackAction2.WasPerformedThisFrame() && cooldownTimer2 <= 0f && canAttack)
            {
                StartChargeAttack();
                //playerAnimator.SetTrigger(PlayerAnimParams.ATTACK);
                //playerAnimator.SetInteger(PlayerAnimParams.ATTACK_INDEX, 2);
            }

            if (inputReader?.AttackAction3 != null && inputReader.AttackAction3.WasPerformedThisFrame() && cooldownTimer3 <= 0f && canAttack)
            {

                playerAnimator.SetTrigger(PlayerAnimParams.ATTACK);
                playerAnimator.SetInteger(PlayerAnimParams.ATTACK_INDEX, 3);
            }

        }

        void StartChargeAttack()
        {
            chargeProjectileInstance = Instantiate(chargeProjectilePrefab, spawnTransform.position, Quaternion.identity);
            chargeProjectileInstance.transform.parent = transform;

            StartCoroutine(StartCharging());
            canAttack = false;
        }

        IEnumerator StartCharging()
        {
            PlayerAttackUI.StartAttackCharge();
            if (chargeProjectileInstance.GetComponent<ProjectileAttack>() != null)
            {
                chargeProjectileInstance.GetComponent<ProjectileAttack>().Charge(maxChargeTimer);
            }
            while (inputReader.AttackAction2.IsPressed())
            {
                if (currentChargeTimer < maxChargeTimer)
                {
                    currentChargeTimer += Time.deltaTime;
                }

                yield return null;
            }
            chargeProjectileInstance.GetComponent<ProjectileAttack>().Release((currentChargeTimer / maxChargeTimer * (maxChargeAttackMultiplier - 1) + 1) * attack2Data.Damage);
            PlayerAttackUI.ReleaseAttack();

            TryAttack(2);
            playerAnimator.SetTrigger(PlayerAnimParams.ATTACK);
            playerAnimator.SetInteger(PlayerAnimParams.ATTACK_INDEX, 2);
        }


        public void TryAttack(int attackID)
        {
            if (IsInputBlocked() || attack1Data == null || hitbox == null)
                return;

            if (cooldownTimer1 > 0f || hitbox.IsActive)
                return;

            switch (attackID)
            {
                case 1:
                    cooldownTimer1 = attack1Data.Cooldown;
                    PlayerAttackUI.OnAttackLaunched(1, attack1Data.Cooldown);
                    break;
                case 2:
                    hitbox.Activate(attack2Data, currentChargeTimer / maxChargeTimer * (maxChargeAttackMultiplier - 1) + 1);
                    currentChargeTimer = 0f;
                    cooldownTimer2 = attack2Data.Cooldown;
                    PlayerAttackUI.OnAttackLaunched(2, attack2Data.Cooldown);
                    break;
                case 3:
                    cooldownTimer3 = attack3Data.Cooldown;
                    PlayerAttackUI.OnAttackLaunched(3, attack3Data.Cooldown);
                    break;
                default:
                    break;
            }

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
                || attack1Data == null;
        }

        public void SetAttackEligibility(int eligibility)
        {
            canAttack = eligibility != 0;
        }
    }
}

using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Combat
{
    /// <summary>
    /// Shared 2.5D ground movement for exploration and combat scenes.
    /// WASD moves on the XZ plane (A/D = X, W/S = Z).
    /// </summary>
    [RequireComponent(typeof(CombatActor))]
    public class PlayerGroundController : MonoBehaviour
    {
        [SerializeField] protected InputReader inputReader;
        [SerializeField] float groundSpeed = 5f;
        [SerializeField] float sprintSpeed = 8f;
        [Tooltip("Depth movement is slower than horizontal to sell the flattened stage perspective.")]
        [SerializeField] float depthSpeedScale = 0.6f;

        protected CombatActor Actor { get; private set; }
        bool gameplayBlocked;
        bool inventoryOpen;
        bool combatEnded;

        protected virtual void Awake()
        {
            Actor = GetComponent<CombatActor>();
        }

        void OnEnable()
        {
            GameEvents.GameplayInputBlocked += OnGameplayInputBlocked;
            GameEvents.InventoryOpenChanged += OnInventoryOpenChanged;
            GameEvents.CombatEnded += OnCombatEnded;
        }

        void OnDisable()
        {
            GameEvents.GameplayInputBlocked -= OnGameplayInputBlocked;
            GameEvents.InventoryOpenChanged -= OnInventoryOpenChanged;
            GameEvents.CombatEnded -= OnCombatEnded;
        }

        protected virtual void Start()
        {
            if (inputReader == null)
                inputReader = FindFirstObjectByType<InputReader>();

            if (inputReader == null)
                Debug.LogError($"[{GetType().Name}] InputReader not found.");
        }

        protected virtual void Update()
        {
            if (IsInputBlocked() || inputReader == null)
                return;

            var moveInput = Read2DInput();
            var sprintInput = ReadSprintInput();
            float targetSpeed = sprintInput == 0 ? groundSpeed : sprintSpeed;

            var groundDelta = new Vector2(
                moveInput.x * targetSpeed,
                moveInput.y * targetSpeed * depthSpeedScale) * Time.deltaTime;

            if (groundDelta.sqrMagnitude > 0f)
            {
                Actor.MoveGround(groundDelta);
                FaceMoveDirection(moveInput.x);
            }
        }

        protected Vector2 Read2DInput()
        {
            return inputReader.MoveAction != null
                ? inputReader.MoveAction.ReadValue<Vector2>().normalized
                : Vector2.zero;
        }

        protected float ReadSprintInput()
        {
            return inputReader.SprintAction != null
                ? inputReader.SprintAction.ReadValue<float>()
                : 0f;
        }


        protected void FaceMoveDirection(float horizontal)
        {
            if (Mathf.Approximately(horizontal, 0f))
                return;

            var scale = transform.localScale;
            scale.x = horizontal > 0f ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        void OnGameplayInputBlocked(bool blocked) => gameplayBlocked = blocked;

        void OnInventoryOpenChanged(bool open) => inventoryOpen = open;

        void OnCombatEnded(CombatResult _) => combatEnded = true;

        protected bool IsInputBlocked() => gameplayBlocked || inventoryOpen || combatEnded;
    }
}

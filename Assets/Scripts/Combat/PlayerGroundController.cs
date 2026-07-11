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
        [Tooltip("Depth movement is slower than horizontal to sell the flattened stage perspective.")]
        [SerializeField] float depthSpeedScale = 0.6f;

        protected CombatActor Actor { get; private set; }
        bool gameplayBlocked;
        bool inventoryOpen;

        protected virtual void Awake()
        {
            Actor = GetComponent<CombatActor>();
        }

        void OnEnable()
        {
            GameEvents.GameplayInputBlocked += OnGameplayInputBlocked;
            GameEvents.InventoryOpenChanged += OnInventoryOpenChanged;
        }

        void OnDisable()
        {
            GameEvents.GameplayInputBlocked -= OnGameplayInputBlocked;
            GameEvents.InventoryOpenChanged -= OnInventoryOpenChanged;
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

            var moveInput = inputReader.MoveAction != null
                ? inputReader.MoveAction.ReadValue<Vector2>()
                : Vector2.zero;

            var groundDelta = new Vector2(
                moveInput.x * groundSpeed,
                moveInput.y * groundSpeed * depthSpeedScale) * Time.deltaTime;

            if (groundDelta.sqrMagnitude > 0f)
            {
                Actor.MoveGround(groundDelta);
                FaceMoveDirection(moveInput.x);
            }
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

        protected bool IsInputBlocked() => gameplayBlocked || inventoryOpen;
    }
}

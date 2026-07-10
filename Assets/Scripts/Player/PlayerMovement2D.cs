using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement2D : MonoBehaviour
    {
        [SerializeField] InputReader inputReader;
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float jumpForce = 10f;
        [SerializeField] Transform groundCheck;
        [SerializeField] float groundCheckRadius = 0.15f;
        [SerializeField] LayerMask groundLayers = ~0;

        Rigidbody2D rb;
        bool gameplayBlocked;
        bool inventoryOpen;
        bool jumpQueued;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.freezeRotation = true;
        }

        void OnEnable()
        {
            GameEvents.GameplayInputBlocked += OnGameplayInputBlocked;
            GameEvents.InventoryOpenChanged += OnInventoryOpenChanged;

            if (inputReader?.JumpAction != null)
                inputReader.JumpAction.performed += OnJumpPerformed;
        }

        void OnDisable()
        {
            GameEvents.GameplayInputBlocked -= OnGameplayInputBlocked;
            GameEvents.InventoryOpenChanged -= OnInventoryOpenChanged;

            if (inputReader?.JumpAction != null)
                inputReader.JumpAction.performed -= OnJumpPerformed;
        }

        void Update()
        {
            if (IsInputBlocked())
                return;

            FlipSprite();
        }

        void FixedUpdate()
        {
            if (IsInputBlocked())
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }

            var moveInput = inputReader != null ? inputReader.MoveAction.ReadValue<Vector2>() : Vector2.zero;
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

            if (jumpQueued && IsGrounded())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpQueued = false;
            }
        }

        void OnJumpPerformed(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            if (!IsInputBlocked())
                jumpQueued = true;
        }

        bool IsGrounded()
        {
            var checkPosition = groundCheck != null ? groundCheck.position : transform.position;
            return Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayers);
        }

        void FlipSprite()
        {
            var moveInput = inputReader != null ? inputReader.MoveAction.ReadValue<Vector2>() : Vector2.zero;
            if (Mathf.Approximately(moveInput.x, 0f))
                return;

            var scale = transform.localScale;
            scale.x = moveInput.x > 0f ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        void OnGameplayInputBlocked(bool blocked)
        {
            gameplayBlocked = blocked;
            if (blocked)
                jumpQueued = false;
        }

        void OnInventoryOpenChanged(bool open) => inventoryOpen = open;

        bool IsInputBlocked() => gameplayBlocked || inventoryOpen;

        void OnDrawGizmosSelected()
        {
            var checkPosition = groundCheck != null ? groundCheck.position : transform.position;
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);
        }
    }
}

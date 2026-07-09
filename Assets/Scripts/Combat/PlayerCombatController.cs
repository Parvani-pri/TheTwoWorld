using TwoWorlds.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TwoWorlds.Combat
{
    /// <summary>
    /// Stage-combat player control: WASD moves on the XZ ground plane
    /// (A/D = X, W/S = Z), hold Jump to rise on Y, hold C/Ctrl to descend.
    /// </summary>
    [RequireComponent(typeof(CombatActor))]
    public class PlayerCombatController : MonoBehaviour
    {
        [SerializeField] InputReader inputReader;
        [SerializeField] float groundSpeed = 5f;
        [Tooltip("Depth movement is slower than horizontal to sell the flattened stage perspective.")]
        [SerializeField] float depthSpeedScale = 0.6f;
        [SerializeField] ArenaBounds bounds;


        [Header("Fly Parameters")]
        [SerializeField] float maxFlightDuration = 5f;
        float remainingFlightDuration;
        float currentFallingSpeed = 0f;
        [SerializeField] float ascendSpeedFactor = 5f;
        [SerializeField] float fallingAcceleration = 7f;

        CombatActor actor;
        bool gameplayBlocked;

        void Awake()
        {
            remainingFlightDuration = maxFlightDuration;
            actor = GetComponent<CombatActor>();
        }

        void OnEnable()
        {
            GameEvents.GameplayInputBlocked += OnGameplayInputBlocked;
        }

        void OnDisable()
        {
            GameEvents.GameplayInputBlocked -= OnGameplayInputBlocked;
        }

        void Start()
        {
            if (inputReader == null)
                inputReader = FindFirstObjectByType<InputReader>();

            if (inputReader == null)
                Debug.LogError("[PlayerCombatController] InputReader not found.");
        }

        void Update()
        {
            if (gameplayBlocked || inputReader == null)
                return;

            var moveInput = inputReader.MoveAction != null
                ? inputReader.MoveAction.ReadValue<Vector2>()
                : Vector2.zero;

            var groundDelta = new Vector2(
                moveInput.x * groundSpeed,
                moveInput.y * groundSpeed * depthSpeedScale) * Time.deltaTime;

            if (groundDelta.sqrMagnitude > 0f)
            {
                actor.MoveGround(groundDelta);
                FaceMoveDirection(moveInput.x);
            }

            var vertical = ReadVerticalInput();
            if (!Mathf.Approximately(vertical, 0f) && remainingFlightDuration >= 0.2 * maxFlightDuration)
            {
                actor.MoveHeight(vertical * ascendSpeedFactor * Time.deltaTime);
                currentFallingSpeed = 0f;
            }

            else if (actor.transform.position.y > bounds.transform.position.y)
                actor.MoveHeight(-(currentFallingSpeed * Time.deltaTime + 0.5f * fallingAcceleration * Time.deltaTime * Time.deltaTime));
                currentFallingSpeed += (fallingAcceleration) * Time.deltaTime;
            if (vertical == 0f && actor.transform.position.y == bounds.transform.position.y)
                RestoreFlightDuration();
        }

        float ReadVerticalInput()
        {
            var value = 0f;

            if (inputReader.JumpAction != null && inputReader.JumpAction.IsPressed() && remainingFlightDuration > 0f)
            {
                value += 1f;
                remainingFlightDuration -= Time.deltaTime;
            }


            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.cKey.isPressed || keyboard.leftCtrlKey.isPressed))
                value -= 1f;

            return value;
        }

        void RestoreFlightDuration()
        {
            currentFallingSpeed = 0f;
            remainingFlightDuration += Time.deltaTime;
            if (remainingFlightDuration > maxFlightDuration)
            {
                remainingFlightDuration = maxFlightDuration;
            }
        }


        void FaceMoveDirection(float horizontal)
        {
            if (Mathf.Approximately(horizontal, 0f))
                return;

            var scale = transform.localScale;
            scale.x = horizontal > 0f ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        void OnGameplayInputBlocked(bool blocked) => gameplayBlocked = blocked;
    }
}

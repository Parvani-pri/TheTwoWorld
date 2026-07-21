using TwoWorlds.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TwoWorlds.Combat
{
    /// <summary>
    /// Combat player control: ground movement from PlayerGroundController plus
    /// flight on Y (Jump to rise, C/Ctrl to descend, gravity when airborne).
    /// </summary>
    public class PlayerCombatController : PlayerGroundController
    {
        [Header("Fly Parameters")]
        [SerializeField] float maxFlightDuration = 5f;
        [SerializeField] float maxSprintDuration = 10f;
        [SerializeField] float ascendSpeedFactor = 5f;
        [SerializeField] float fallingAcceleration = 7f;

        [SerializeField] Animator animator;

        public float remainingFlightDuration { get; private set; }
        public float remainingSprintDuration { get; private set; }
        float currentFallingSpeed;

        protected override void Awake()
        {
            base.Awake();
            remainingFlightDuration = maxFlightDuration;
            remainingSprintDuration = maxSprintDuration;
        }

        protected override void Update()
        {
            if (IsInputBlocked() || inputReader == null)
            {
                if (animator != null)
                    animator.SetFloat(PlayerAnimParams.SPEED, 0f);

                return;
            }

            base.Update();
            float speedValue = Read2DInput() == Vector2.zero ? 0f : ReadSprintInput() == 0f && remainingSprintDuration > 0f ? 0.5f : 1f;

            animator.SetFloat(PlayerAnimParams.SPEED, speedValue);

            if (speedValue == 1)
            {
                remainingSprintDuration -= Time.deltaTime;
                GameEvents.UpdateSprintBar(remainingSprintDuration, maxSprintDuration);
            }
            else if (remainingSprintDuration < maxSprintDuration)
            {
                RestoreSprintDuration();
            }

            if (!IsAttacking)
                UpdateFlight();
        }

        bool IsAttacking
        {
            get
            {
                var attack = GetComponent<IPlayerAttackState>();
                return attack != null && attack.IsAttacking;
            }
        }

        void UpdateFlight()
        {
            var vertical = ReadVerticalInput();

            if (!Mathf.Approximately(vertical, 0f) && remainingFlightDuration >= 0.2f * maxFlightDuration)
            {
                Actor.MoveHeight(vertical * ascendSpeedFactor * Time.deltaTime);
                currentFallingSpeed = 0f;
            }
            else if (Actor.Height > 0.05f)
            {
                Actor.MoveHeight(-(currentFallingSpeed * Time.deltaTime
                    + 0.5f * fallingAcceleration * Time.deltaTime * Time.deltaTime));
                currentFallingSpeed += fallingAcceleration * Time.deltaTime;
            }

            if (Mathf.Approximately(vertical, 0f) && Actor.Height <= 0.05f && remainingFlightDuration <= maxFlightDuration)
                RestoreFlightDuration();
        }

        float ReadVerticalInput()
        {
            var value = 0f;

            if (inputReader.JumpAction != null && inputReader.JumpAction.IsPressed() && remainingFlightDuration > 0f)
            {
                value += 1f;
                remainingFlightDuration -= Time.deltaTime;
                GameEvents.UpdateFlightBar(remainingFlightDuration, maxFlightDuration);
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
                remainingFlightDuration = maxFlightDuration;
            GameEvents.UpdateFlightBar(remainingFlightDuration, maxFlightDuration);

        }

        void RestoreSprintDuration()
        {
            remainingSprintDuration += Time.deltaTime;
            if (remainingSprintDuration > maxSprintDuration)
                remainingSprintDuration = maxSprintDuration;
            GameEvents.UpdateSprintBar(remainingSprintDuration, maxSprintDuration);
        }
    }
}

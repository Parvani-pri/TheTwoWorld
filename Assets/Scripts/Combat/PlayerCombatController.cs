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
        [SerializeField] float ascendSpeedFactor = 5f;
        [SerializeField] float fallingAcceleration = 7f;

        float remainingFlightDuration;
        float currentFallingSpeed;

        protected override void Awake()
        {
            base.Awake();
            remainingFlightDuration = maxFlightDuration;
        }

        protected override void Update()
        {
            if (IsInputBlocked() || inputReader == null)
                return;

            base.Update();
            UpdateFlight();
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

            if (Mathf.Approximately(vertical, 0f) && Actor.Height <= 0.05f)
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
                remainingFlightDuration = maxFlightDuration;
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

namespace TwoWorlds.Core
{
    /// <summary>
    /// Reads actions from InputSystem_Actions.inputactions assigned in the Inspector.
    /// </summary>
    public class InputReader : MonoBehaviour
    {
        [SerializeField] InputActionAsset inputActions;

        InputAction interactAction;
        InputAction submitAction;
        InputAction cancelAction;
        InputAction inventoryAction;
        InputAction moveAction;
        InputAction jumpAction;
        InputAction attackAction1;
        InputAction attackAction2;
        InputAction attackAction3;
        InputAction sprintAction;
        InputAction toggleMenu;

        public InputAction InteractAction => interactAction;
        public InputAction SubmitAction => submitAction;
        public InputAction CancelAction => cancelAction;
        public InputAction InventoryAction => inventoryAction;
        public InputAction MoveAction => moveAction;
        public InputAction JumpAction => jumpAction;
        public InputAction AttackAction1 => attackAction1;
        public InputAction AttackAction2 => attackAction2;
        public InputAction AttackAction3 => attackAction3;
        public InputAction SprintAction => sprintAction;
        public InputAction ToggleMenu => toggleMenu;



        void Awake()
        {
            if (inputActions == null)
            {
                Debug.LogError("[InputReader] Assign InputSystem_Actions in the Inspector.");
                return;
            }

            var playerMap = inputActions.FindActionMap("Player", true);
            var uiMap = inputActions.FindActionMap("UI", true);

            interactAction = playerMap.FindAction("Interact", true);
            submitAction = uiMap.FindAction("Submit", true);
            cancelAction = uiMap.FindAction("Cancel", true);
            toggleMenu = uiMap.FindAction("ToggleMenu", true);

            inventoryAction = playerMap.FindAction("Inventory", true);
            moveAction = playerMap.FindAction("Move", true);
            jumpAction = playerMap.FindAction("Jump", true);
            attackAction1 = playerMap.FindAction("Attack1", true);
            attackAction2 = playerMap.FindAction("Attack2", true);
            attackAction3 = playerMap.FindAction("Attack3", true);
            sprintAction = playerMap.FindAction("Sprint", true);

        }

        void OnEnable()
        {
            inputActions?.Enable();
        }

        void OnDisable()
        {
            inputActions?.Disable();
        }
    }
}

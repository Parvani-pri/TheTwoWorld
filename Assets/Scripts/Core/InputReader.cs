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
        InputAction attackAction;
        InputAction sprintAction;

        public InputAction InteractAction => interactAction;
        public InputAction SubmitAction => submitAction;
        public InputAction CancelAction => cancelAction;
        public InputAction InventoryAction => inventoryAction;
        public InputAction MoveAction => moveAction;
        public InputAction JumpAction => jumpAction;
        public InputAction AttackAction => attackAction;
        public InputAction SprintAction => sprintAction;



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

            inventoryAction = playerMap.FindAction("Inventory", true);
            moveAction = playerMap.FindAction("Move", true);
            jumpAction = playerMap.FindAction("Jump", true);
            attackAction = playerMap.FindAction("Attack", true);
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

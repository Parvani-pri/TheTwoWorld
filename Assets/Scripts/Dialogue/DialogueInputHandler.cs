using TwoWorlds.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TwoWorlds.Dialogue
{
    public class DialogueInputHandler : MonoBehaviour
    {
        [SerializeField] InputReader inputReader;

        void Start()
        {
            if (inputReader == null)
                inputReader = FindFirstObjectByType<InputReader>();

            if (inputReader?.InteractAction != null)
                inputReader.InteractAction.performed += OnAdvance;

            if (inputReader?.SubmitAction != null)
                inputReader.SubmitAction.performed += OnAdvance;
        }

        void OnDestroy()
        {
            if (inputReader?.InteractAction != null)
                inputReader.InteractAction.performed -= OnAdvance;

            if (inputReader?.SubmitAction != null)
                inputReader.SubmitAction.performed -= OnAdvance;
        }

        void OnAdvance(InputAction.CallbackContext _)
        {
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying)
                DialogueManager.Instance.AdvanceDialogue();
        }
    }
}

using UnityEngine;

namespace TwoWorlds.Dialogue
{
    /// <summary>
    /// Drives a bool walk parameter from scripted dialogue movement.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class StoryNpcAnimator : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] DialogueActorController actorController;
        [SerializeField] string walkBoolParameter = "isWalk";

        int walkBoolHash;

        void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            if (actorController == null)
                actorController = GetComponent<DialogueActorController>();

            walkBoolHash = Animator.StringToHash(walkBoolParameter);
        }

        void Update()
        {
            if (animator == null)
                return;

            var isWalking = actorController != null && actorController.IsMoving;
            animator.SetBool(walkBoolHash, isWalking);
        }
    }
}

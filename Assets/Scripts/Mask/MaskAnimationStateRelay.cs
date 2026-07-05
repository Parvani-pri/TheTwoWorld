using UnityEngine;

namespace XuFu.MaskSystem
{
    // Add this StateMachineBehaviour to each Animator state.
    // It tells MaskController which JSON/profile to use, e.g. idle, walk, run, jump, attack1.
    public class MaskAnimationStateRelay : StateMachineBehaviour
    {
        public string animationName = "idle";

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var controller = animator.GetComponent<MaskController>();
            if (controller != null)
                controller.SetAnimation(animationName);
        }
    }
}

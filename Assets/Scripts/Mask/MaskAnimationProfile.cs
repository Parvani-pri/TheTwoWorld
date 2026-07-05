using UnityEngine;

namespace XuFu.MaskSystem
{
    public class MaskAnimationProfile : ScriptableObject
    {
        [Header("Animation Identity")]
        [Tooltip("Must match MaskAnimationStateRelay.animationName exactly, e.g. idle, walk, run, jump, attack1.")]
        public string animationName = "idle";

        [Header("Body Frames")]
        [Tooltip("Drag the sliced body sprites here in JSON frame order: 0, 1, 2...")]
        public Sprite[] bodyFrames;

        [Header("Anchor JSON")]
        public TextAsset anchorJson;

        [Header("Fallback For JSON Without Rotation")]
        [Tooltip("Only used by old JSON files that do not contain angle/maskBaseAngle.")]
        public float defaultAngle = 0f;

        public int FrameCount => bodyFrames == null ? 0 : bodyFrames.Length;
    }
}

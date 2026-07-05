using UnityEngine;

namespace XuFu.MaskSystem
{
    public class MaskDatabase : ScriptableObject
    {
        public MaskAnimationProfile[] profiles;

        public MaskAnimationProfile GetProfile(string animationName)
        {
            if (profiles == null || string.IsNullOrEmpty(animationName)) return null;

            string target = animationName.Trim().ToLowerInvariant();
            foreach (var p in profiles)
            {
                if (p == null || string.IsNullOrEmpty(p.animationName)) continue;
                if (p.animationName.Trim().ToLowerInvariant() == target)
                    return p;
            }
            return null;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace XuFu.MaskSystem
{
    public class MaskController : MonoBehaviour
    {
        [Header("Animation Follow Strength")]
        public float runFollowStrength;
        public float jumpFollowStrength;
        public float attack1FollowStrength;
        public float attack2FollowStrength;
        public float attack3FollowStrength;

        [Header("Animator Blend Tree")]
        public Animator animator;
        public string speedParameter = "speed";
        public float walkThreshold = 0.25f;
        public float runThreshold = 0.75f;

        [Header("Scene References")]
        public SpriteRenderer bodyRenderer;
        public SpriteRenderer maskRenderer;

        [Header("Data")]
        public MaskDatabase database;
        public string currentAnimation = "idle";
        public MaskItem equippedMask;

        [Header("Optional: Up To 3 Masks")]
        public MaskItem[] quickMasks;

        [Header("Settings")]
        public bool hideMaskWhenNoFrameFound = true;

        private MaskAnimationProfile currentProfile;
        private MaskAnchorData currentAnchorData;
        private Vector3 baseMaskLocalPosition;
        private bool hasBaseMaskPosition = false;
        private readonly Dictionary<Sprite, int> spriteToFrame = new Dictionary<Sprite, int>();

        void Start()
        {
            SetAnimation(currentAnimation);
            EquipMask(equippedMask);
        }

        void LateUpdate()
        {
            UpdateAnimationFromAnimator();
            UpdateMaskFromCurrentBodySprite();
            
        }

        private void UpdateAnimationFromAnimator()
        {
            if (animator == null)
                return;

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName("jump"))
            {
                SetIfChanged("jump");
                return;
            }

            if (state.IsName("attack1") || state.IsName("attack2") || state.IsName("attack3"))
            {
                int attackIndex = animator.GetInteger("attackIndex");

                if (attackIndex <= 1)
                    SetIfChanged("attack1");
                else if (attackIndex == 2)
                    SetIfChanged("attack2");
                else
                    SetIfChanged("attack3");

                return;
            }

            if (state.IsName("Locomotion"))
            {
                float speed = animator.GetFloat(speedParameter);

                if (speed < walkThreshold)
                    SetIfChanged("idle");
                else if (speed < runThreshold)
                    SetIfChanged("walk");
                else
                    SetIfChanged("run");
            }
        }

private void SetIfChanged(string animName)
{
    if (currentAnimation != animName)
        SetAnimation(animName);
}

        public void SetAnimation(string animationName)
        {
            currentAnimation = animationName;
            currentProfile = database != null ? database.GetProfile(animationName) : null;

            currentAnchorData = null;
            spriteToFrame.Clear();

            if (currentProfile == null)
            {
                Debug.LogWarning($"MaskController: No profile found for '{animationName}'.", this);
                return;
            }

            if (currentProfile.anchorJson != null)
            {
                currentAnchorData = JsonUtility.FromJson<MaskAnchorData>(currentProfile.anchorJson.text);
            }
            else
            {
                Debug.LogWarning($"MaskController: Profile '{animationName}' has no JSON.", currentProfile);
            }

            if (currentProfile.bodyFrames != null)
            {
                for (int i = 0; i < currentProfile.bodyFrames.Length; i++)
                {
                    Sprite s = currentProfile.bodyFrames[i];
                    if (s != null && !spriteToFrame.ContainsKey(s))
                        spriteToFrame.Add(s, i);
                }
            }
        }

        public void EquipMask(MaskItem mask)
        {
            equippedMask = mask;

            if (maskRenderer == null)
                return;

            maskRenderer.sprite = equippedMask != null ? equippedMask.maskSprite : null;
            maskRenderer.enabled = equippedMask != null && equippedMask.maskSprite != null;
            baseMaskLocalPosition = maskRenderer.transform.localPosition;
            hasBaseMaskPosition = true;
        }

        public void SetMaskIndex(int index)
        {
            if (index <= 0)
            {
                EquipMask(null);
                return;
            }

            int arrayIndex = index - 1;

            if (quickMasks == null || arrayIndex < 0 || arrayIndex >= quickMasks.Length)
            {
                Debug.LogWarning($"MaskController: quickMasks has no mask at index {index}.", this);
                return;
            }

            EquipMask(quickMasks[arrayIndex]);
        }

        private void UpdateMaskFromCurrentBodySprite()
        {
            if (bodyRenderer == null || maskRenderer == null)
                return;

            if (equippedMask == null || equippedMask.maskSprite == null)
                return;

            if (currentAnchorData == null || currentAnchorData.anchors == null)
                return;

            Sprite currentBodySprite = bodyRenderer.sprite;

            if (currentBodySprite == null)
                return;

            if (!spriteToFrame.TryGetValue(currentBodySprite, out int frameIndex))
            {
                //if (hideMaskWhenNoFrameFound)
                    //maskRenderer.enabled = false;

                return;
            }

            ApplyFrame(frameIndex);
        }

        private void ApplyFrame(int frameIndex)
        {
           if (currentAnchorData == null || currentAnchorData.anchors == null)
                return;

            if (frameIndex < 0 || frameIndex >= currentAnchorData.anchors.Length)
                return;

            if (bodyRenderer == null || maskRenderer == null)
                return;

            if (equippedMask == null || equippedMask.maskSprite == null)
                return;

            AnchorFrame a = currentAnchorData.anchors[frameIndex];

            maskRenderer.enabled = true;
            Sprite bodySprite = bodyRenderer.sprite;
            if (bodySprite == null)
                return;

            float ppu = bodySprite.pixelsPerUnit;

            AnchorFrame baseAnchor = currentAnchorData.anchors[0];

            float dx = a.face.x - baseAnchor.face.x;
            float dy = a.face.y - baseAnchor.face.y;

            // 可調，jump/attack 太誇張就較細
            //float followStrength = 0.35f;
            float followStrength = 1f;

            if (currentAnimation == "run")
                followStrength = runFollowStrength;
            else if (currentAnimation == "jump")
                followStrength = jumpFollowStrength;
            else if (currentAnimation == "attack1")
                followStrength = attack1FollowStrength;
            else if (currentAnimation == "attack2")
                followStrength = attack2FollowStrength;     
            else if (currentAnimation == "attack3")
                followStrength = attack3FollowStrength;          

            Vector3 offset = new Vector3(
                dx / ppu * followStrength,
                -dy / ppu * followStrength,
                0f
            );

            maskRenderer.transform.localPosition = baseMaskLocalPosition + offset;
        }

        private Vector2 GetMaskPivot(Vector2 fallbackPivot)
        {
            if (currentAnchorData != null &&
                currentAnchorData.maskPivot != null)
            {
                return new Vector2(
                    currentAnchorData.maskPivot.x,
                    currentAnchorData.maskPivot.y
                );
            }

            if (currentAnchorData != null &&
                currentAnchorData.maskPreview != null &&
                currentAnchorData.maskPreview.maskPivotOriginalPixels != null)
            {
                return new Vector2(
                    currentAnchorData.maskPreview.maskPivotOriginalPixels.x,
                    currentAnchorData.maskPreview.maskPivotOriginalPixels.y
                );
            }

            return fallbackPivot;
        }

        private bool IsZero(float x, float y)
        {
            return Mathf.Approximately(x, 0f) && Mathf.Approximately(y, 0f);
        }
    }

}
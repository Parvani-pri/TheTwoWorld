using System.Collections.Generic;
using UnityEngine;

namespace XuFu.MaskSystem
{
    public class MaskController : MonoBehaviour
    {
        [Header("Scene References")]
        public SpriteRenderer bodyRenderer;
        public SpriteRenderer maskRenderer;

        [Header("Data")]
        public MaskDatabase database;
        public string currentAnimation = "idle";
        public MaskItem equippedMask;

        [Header("Optional: Up To 3 Masks")]
        [Tooltip("Optional shortcut list. 0 can mean no mask; 1-3 can be your mask items.")]
        public MaskItem[] quickMasks;

        [Header("Import Settings")]
        public float pixelsPerUnit = 640f;
        public bool hideMaskWhenNoFrameFound = true;

        private MaskAnimationProfile currentProfile;
        private MaskAnchorData currentAnchorData;
        private readonly Dictionary<Sprite, int> spriteToFrame = new Dictionary<Sprite, int>();

        void Start()
        {
            SetAnimation(currentAnimation);
            EquipMask(equippedMask);
        }

        void LateUpdate()
        {
            UpdateMaskFromCurrentBodySprite();
        }

        public void SetAnimation(string animationName)
        {
            currentAnimation = animationName;
            currentProfile = database != null ? database.GetProfile(animationName) : null;
            currentAnchorData = null;
            spriteToFrame.Clear();

            if (currentProfile == null)
            {
                Debug.LogWarning($"MaskController: No MaskAnimationProfile found for animation '{animationName}'. Check MaskDatabase.profiles and MaskAnimationStateRelay.animationName.", this);
                return;
            }

            if (currentProfile.anchorJson != null)
                currentAnchorData = JsonUtility.FromJson<MaskAnchorData>(currentProfile.anchorJson.text);
            else
                Debug.LogWarning($"MaskController: Profile '{animationName}' has no anchorJson.", currentProfile);

            if (currentProfile.bodyFrames != null)
            {
                for (int i = 0; i < currentProfile.bodyFrames.Length; i++)
                {
                    var s = currentProfile.bodyFrames[i];
                    if (s != null && !spriteToFrame.ContainsKey(s))
                        spriteToFrame.Add(s, i);
                }
            }
        }

        public void EquipMask(MaskItem mask)
        {
            equippedMask = mask;
            if (maskRenderer == null) return;

            maskRenderer.sprite = equippedMask != null ? equippedMask.maskSprite : null;
            maskRenderer.enabled = equippedMask != null && equippedMask.maskSprite != null;
        }

        // Convenience for your case: 0 = no mask, 1/2/3 = quickMasks[0/1/2].
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

        public void UpdateMaskFromCurrentBodySprite()
        {
            if (bodyRenderer == null || maskRenderer == null || equippedMask == null || equippedMask.maskSprite == null)
                return;

            if (currentAnchorData == null || currentAnchorData.anchors == null)
                return;

            if (!spriteToFrame.TryGetValue(bodyRenderer.sprite, out int frameIndex))
            {
                if (hideMaskWhenNoFrameFound) maskRenderer.enabled = false;
                return;
            }

            ApplyFrame(frameIndex);
        }

        public void ApplyFrame(int frameIndex)
        {
            if (currentAnchorData == null || currentAnchorData.anchors == null) return;
            if (frameIndex < 0 || frameIndex >= currentAnchorData.anchors.Length) return;

            maskRenderer.enabled = true;

            AnchorFrame a = currentAnchorData.anchors[frameIndex];

            float fw = currentAnchorData.frameWidth > 0 ? currentAnchorData.frameWidth : 640f;
            float fh = currentAnchorData.frameHeight > 0 ? currentAnchorData.frameHeight : 640f;

            float x = a.face.x + equippedMask.extraOffsetPixels.x;
            float y = a.face.y + equippedMask.extraOffsetPixels.y;

            float localX = (x - fw * 0.5f) / pixelsPerUnit;
            float localY = -(y - fh * 0.5f) / pixelsPerUnit;
            maskRenderer.transform.localPosition = new Vector3(localX, localY, maskRenderer.transform.localPosition.z);

            bool jsonHasRotation = Mathf.Abs(currentAnchorData.maskBaseAngle) > 0.0001f || Mathf.Abs(a.angle) > 0.0001f;
            float finalAngle = jsonHasRotation ? (a.angle - currentAnchorData.maskBaseAngle) : currentProfile.defaultAngle;
            finalAngle += equippedMask.extraRotation;
            maskRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, finalAngle);

            float frameScale = Mathf.Approximately(a.scale, 0f) ? 1f : a.scale;
            float s = frameScale * equippedMask.globalScale;
            maskRenderer.transform.localScale = new Vector3(s, s, 1f);
        }
    }
}

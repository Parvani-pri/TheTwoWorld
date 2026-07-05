using UnityEngine;

namespace XuFu.MaskSystem
{
    public class MaskItem : ScriptableObject
    {
        public string maskId = "bird_mask";
        public Sprite maskSprite;

        [Header("Tuning")]
        [Tooltip("Use this to make one mask slightly bigger/smaller without changing the PNG.")]
        public float globalScale = 1f;
        [Tooltip("Pixel offset after anchor position. +X moves right, +Y moves down in image-space.")]
        public Vector2 extraOffsetPixels = Vector2.zero;
        [Tooltip("Extra rotation in degrees after JSON rotation is applied.")]
        public float extraRotation = 0f;
    }
}

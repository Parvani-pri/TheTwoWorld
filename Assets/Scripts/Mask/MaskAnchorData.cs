using System;
using UnityEngine;

namespace XuFu.MaskSystem
{
    [Serializable]
    public class MaskAnchorData
    {
        public string animation;
        public int frameWidth;
        public int frameHeight;
        public Vec2Json maskPivot;
        public float maskBaseAngle;
        public AnchorFrame[] anchors;
    }

    [Serializable]
    public class AnchorFrame
    {
        public int frame;
        public FacePoint face;
        public float angle;
        public float scale = 1f;
        public float confidence = 1f;
    }

    [Serializable]
    public class FacePoint
    {
        public float x;
        public float y;
    }

    [Serializable]
    public class Vec2Json
    {
        public float x;
        public float y;
    }
}

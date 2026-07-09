using UnityEngine;

namespace TwoWorlds.Combat
{
    /// <summary>
    /// Rectangular ground stage on the XZ plane. groundSize.x = width (X),
    /// groundSize.y = depth (Z). Actors fly upward on world Y.
    /// </summary>
    public class ArenaBounds : MonoBehaviour
    {
        [SerializeField] Vector2 groundSize = new(12f, 4f);
        [SerializeField] float maxFlightHeight = 3f;

        public float MaxFlightHeight => maxFlightHeight;
        public float GroundLevelY => transform.position.y;

        public Vector2 ClampGroundXZ(Vector2 groundXZ) =>
            ClampGroundXZ(groundXZ.x, groundXZ.y);

        public Vector2 ClampGroundXZ(float x, float z)
        {
            var center = transform.position;
            var half = groundSize * 0.5f;

            return new Vector2(
                Mathf.Clamp(x, center.x - half.x, center.x + half.x),
                Mathf.Clamp(z, center.z - half.y, center.z + half.y));
        }

        void OnDrawGizmos()
        {
            var center = transform.position;
            var floorSize = new Vector3(groundSize.x, 0.05f, groundSize.y);

            Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.9f);
            Gizmos.DrawWireCube(center, floorSize);

            Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.6f);
            Gizmos.DrawWireCube(center + Vector3.up * maxFlightHeight, floorSize);
        }
    }
}

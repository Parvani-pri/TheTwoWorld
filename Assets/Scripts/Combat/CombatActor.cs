using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace TwoWorlds.Combat
{
    public enum CombatFaction
    {
        Player,
        Enemy
    }

    /// <summary>
    /// Core 2.5D combat unit. Ground movement lives on the XZ plane;
    /// flight height is world Y above the arena floor.
    /// GroundPosition: x = world X, y = world Z (depth).
    /// </summary>
    public class CombatActor : MonoBehaviour
    {
        static readonly List<CombatActor> active = new();
        public static IReadOnlyList<CombatActor> Active => active;

        [SerializeField] CombatFaction faction = CombatFaction.Enemy;
        [SerializeField] ArenaBounds arena;
        [Tooltip("Optional. Sorting order follows stage depth (Z) so closer actors draw on top.")]
        [SerializeField] SpriteRenderer depthSortedRenderer;
        [SerializeField] float depthSortingScale = 100f;

        private bool isTransformLocked = false;

        public CombatFaction Faction => faction;
        public ArenaBounds Arena => arena;
        /// <summary>Ground plane coords: x = world X, y = world Z.</summary>
        public Vector2 GroundPosition { get; private set; }
        public float Height { get; private set; }
        public bool IsFlying => Height > 0.05f;

        void Awake()
        {
            if (arena == null)
                arena = FindFirstObjectByType<ArenaBounds>();

            var groundY = GetGroundLevelY();
            GroundPosition = ClampGround(new Vector2(transform.position.x, transform.position.z));
            Height = Mathf.Max(0f, transform.position.y - groundY);
            ApplyVisualPosition();
        }

        void OnEnable() => active.Add(this);
        void OnDisable() => active.Remove(this);



        /// <summary>Delta on the ground plane: x = X, y = Z.</summary>
        public void MoveGround(Vector2 delta)
        {
            if (!isTransformLocked)
            {
                GroundPosition = ClampGround(GroundPosition + delta);
                ApplyVisualPosition();
            }
        }

        public void MoveHeight(float delta) => SetHeight(Height + delta);

        public void SetHeight(float height)
        {
            var maxHeight = arena != null ? arena.MaxFlightHeight : float.PositiveInfinity;
            Height = Mathf.Clamp(height, 0f, maxHeight);
            ApplyVisualPosition();
        }

        public static CombatActor FindClosest(CombatFaction faction, Vector2 fromGroundPosition)
        {
            CombatActor closest = null;
            var closestSqr = float.PositiveInfinity;

            foreach (var actor in active)
            {
                if (actor.faction != faction)
                    continue;

                var sqr = (actor.GroundPosition - fromGroundPosition).sqrMagnitude;
                if (sqr < closestSqr)
                {
                    closestSqr = sqr;
                    closest = actor;
                }
            }

            return closest;
        }

        public float GetGroundLevelY() =>
            arena != null ? arena.GroundLevelY : transform.position.y - Height;

        Vector2 ClampGround(Vector2 groundXZ) =>
            arena != null ? arena.ClampGroundXZ(groundXZ) : groundXZ;

        void ApplyVisualPosition()
        {
            transform.position = new Vector3(
                GroundPosition.x,
                GetGroundLevelY() + Height,
                GroundPosition.y);
            if (depthSortedRenderer != null)
                depthSortedRenderer.sortingOrder = Mathf.RoundToInt(-GroundPosition.y * depthSortingScale);
        }

        public void SetTransformLock(int isLocked)
        {
            isTransformLocked = isLocked == 0 ? false : true;
        }
        public void SetGroundPosition(Vector2 groundPosition)
        {
            GroundPosition = groundPosition;
        }

        /// <summary>
        /// Updates internal ground/height state from the current transform without moving it.
        /// Use after external systems (e.g. scripted dialogue moves) set transform.position directly.
        /// </summary>
        public void SyncGroundFromTransform()
        {
            GroundPosition = ClampGround(new Vector2(transform.position.x, transform.position.z));
            var groundY = GetGroundLevelY();
            Height = Mathf.Max(0f, transform.position.y - groundY);
            if (depthSortedRenderer != null)
                depthSortedRenderer.sortingOrder = Mathf.RoundToInt(-GroundPosition.y * depthSortingScale);
        }
    }
}


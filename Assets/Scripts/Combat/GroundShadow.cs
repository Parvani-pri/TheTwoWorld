using UnityEngine;

namespace TwoWorlds.Combat
{
    /// <summary>
    /// Keeps a shadow sprite on the ground (XZ at floor Y) while the actor
    /// rises on world Y when flying.
    /// </summary>
    [RequireComponent(typeof(CombatActor))]
    public class GroundShadow : MonoBehaviour
    {
        [SerializeField] SpriteRenderer shadowRenderer;
        [SerializeField] float minScale = 0.5f;
        [SerializeField] float minAlpha = 0.25f;

        CombatActor actor;
        Vector3 baseScale;
        float baseAlpha;

        void Awake()
        {
            actor = GetComponent<CombatActor>();

            if (shadowRenderer != null)
            {
                baseScale = shadowRenderer.transform.localScale;
                baseAlpha = shadowRenderer.color.a;
            }
        }

        void LateUpdate()
        {
            if (shadowRenderer == null)
                return;

            shadowRenderer.transform.position = new Vector3(
                actor.GroundPosition.x,
                actor.GetGroundLevelY(),
                actor.GroundPosition.y);

            var maxHeight = actor.Arena != null ? actor.Arena.MaxFlightHeight : 3f;
            var t = maxHeight > 0f ? Mathf.Clamp01(actor.Height / maxHeight) : 0f;

            shadowRenderer.transform.localScale = baseScale * Mathf.Lerp(1f, minScale, t);

            var color = shadowRenderer.color;
            color.a = Mathf.Lerp(baseAlpha, minAlpha, t);
            shadowRenderer.color = color;
        }
    }
}

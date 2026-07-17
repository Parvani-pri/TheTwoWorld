using UnityEngine;

namespace TwoWorlds.Dialogue
{
    public class DialogueMoveMarker : MonoBehaviour
    {
        [SerializeField] string markerId;

        public string MarkerId => string.IsNullOrWhiteSpace(markerId) ? name : markerId.Trim();
        public Vector3 WorldPosition => transform.position;

        void OnEnable() => DialogueActorRegistry.Register(this);

        void OnDisable() => DialogueActorRegistry.Unregister(this);

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.85f);
            Gizmos.DrawWireSphere(transform.position, 0.2f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.35f, MarkerId);
        }
#endif
    }
}

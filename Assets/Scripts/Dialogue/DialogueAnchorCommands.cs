using UnityEngine;

namespace TwoWorlds.Dialogue
{
    public static class DialogueAnchorCommands
    {
        public const string MainLobbyXuFuMarkerId = "xufu";
        public const string MainLobbyXiaomeiMarkerId = "xiaomei";

        public static void StageMainLobbyXuFuAndXiaomei(bool includeXiaomei = true)
        {
            MoveActorToMarker("xufu", MainLobbyXuFuMarkerId);

            if (includeXiaomei)
                MoveActorToMarker("xiaomei", MainLobbyXiaomeiMarkerId);
        }

        public static void MoveActorToMarker(string actorKey, string markerId)
        {
            if (!DialogueActorRegistry.TryGetActor(actorKey, out var actor))
            {
                Debug.LogWarning($"[DialogueAnchorCommands] Actor not found: {actorKey}");
                return;
            }

            if (!DialogueActorRegistry.TryGetMarker(markerId, out var marker))
            {
                Debug.LogWarning($"[DialogueAnchorCommands] Marker not found: {markerId}");
                return;
            }

            actor.MoveTo(marker.WorldPosition);
        }
    }
}

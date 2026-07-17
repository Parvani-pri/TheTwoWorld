using System.Collections.Generic;

namespace TwoWorlds.Dialogue
{
    public static class DialogueActorRegistry
    {
        static readonly Dictionary<string, DialogueActorController> actors =
            new(System.StringComparer.OrdinalIgnoreCase);

        static readonly Dictionary<string, DialogueMoveMarker> markers =
            new(System.StringComparer.OrdinalIgnoreCase);

        public static void Register(DialogueActorController actor)
        {
            if (actor == null)
                return;

            actors[actor.ActorKey] = actor;
        }

        public static void Unregister(DialogueActorController actor)
        {
            if (actor == null)
                return;

            if (actors.TryGetValue(actor.ActorKey, out var current) && current == actor)
                actors.Remove(actor.ActorKey);
        }

        public static void Register(DialogueMoveMarker marker)
        {
            if (marker == null)
                return;

            markers[marker.MarkerId] = marker;
        }

        public static void Unregister(DialogueMoveMarker marker)
        {
            if (marker == null)
                return;

            if (markers.TryGetValue(marker.MarkerId, out var current) && current == marker)
                markers.Remove(marker.MarkerId);
        }

        public static bool TryGetActor(string actorKey, out DialogueActorController actor)
        {
            actor = null;
            if (string.IsNullOrWhiteSpace(actorKey))
                return false;

            return actors.TryGetValue(actorKey.Trim(), out actor) && actor != null;
        }

        public static bool TryGetMarker(string markerId, out DialogueMoveMarker marker)
        {
            marker = null;
            if (string.IsNullOrWhiteSpace(markerId))
                return false;

            return markers.TryGetValue(markerId.Trim(), out marker) && marker != null;
        }

        public static void ClearScriptedOverrides()
        {
            foreach (var pair in actors)
            {
                if (pair.Value != null)
                    pair.Value.ClearScriptedOverride();
            }
        }
    }
}

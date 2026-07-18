using TwoWorlds.Core;
using TwoWorlds.Inventory;
using TwoWorlds.Progress;
using UnityEngine;

namespace TwoWorlds.Dialogue
{
    public class DialogueActionRunner : MonoBehaviour
    {
        [SerializeField] GameProgress gameProgress;

        void Awake()
        {
            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        void OnEnable()
        {
            GameEvents.DialogueLineShown += OnDialogueLineShown;
            GameEvents.ScriptDialogueEnded += OnScriptDialogueEnded;
            GameEvents.DialogueEnded += OnDialogueEndedWithoutInfo;
        }

        void OnDisable()
        {
            GameEvents.DialogueLineShown -= OnDialogueLineShown;
            GameEvents.ScriptDialogueEnded -= OnScriptDialogueEnded;
            GameEvents.DialogueEnded -= OnDialogueEndedWithoutInfo;
            DialogueActorRegistry.ClearScriptedOverrides();
        }

        void OnDialogueLineShown(DialogueLineShownInfo info)
        {
            if (!info.HasLine || string.IsNullOrWhiteSpace(info.Action))
                return;

            ExecuteActions(info.Action, info.Interactor);
        }

        void OnScriptDialogueEnded(DialogueEndInfo _) => DialogueActorRegistry.ClearScriptedOverrides();

        void OnDialogueEndedWithoutInfo() => DialogueActorRegistry.ClearScriptedOverrides();

        void ExecuteActions(string actionText, GameObject interactor)
        {
            var parts = actionText.Split(';');
            foreach (var rawPart in parts)
            {
                var part = rawPart.Trim();
                if (part.Length == 0)
                    continue;

                ExecuteAction(part, interactor);
            }
        }

        void ExecuteAction(string action, GameObject interactor)
        {
            var separatorIndex = action.IndexOf(':');
            if (separatorIndex <= 0)
            {
                Debug.LogWarning($"[DialogueActionRunner] Invalid action: {action}");
                return;
            }

            var verb = action.Substring(0, separatorIndex).Trim().ToLowerInvariant();
            var payload = action.Substring(separatorIndex + 1).Trim();

            switch (verb)
            {
                case "move":
                    ExecuteMove(payload);
                    break;
                case "face":
                    ExecuteFace(payload, interactor);
                    break;
                case "stop":
                    ExecuteStop(payload);
                    break;
                case "setflag":
                    ExecuteSetFlag(payload, true);
                    break;
                case "clearflag":
                    ExecuteSetFlag(payload, false);
                    break;
                case "unlockenteryin":
                    ExecuteUnlockEnterYin(payload);
                    break;
                case "unlockexitbattle":
                    ExecuteUnlockExitBattle(payload);
                    break;
                default:
                    Debug.LogWarning($"[DialogueActionRunner] Unknown action verb: {verb}");
                    break;
            }
        }

        void ExecuteMove(string payload)
        {
            if (!TryParseActorTarget(payload, out var actorKey, out var markerId))
                return;

            DialogueAnchorCommands.MoveActorToMarker(actorKey, markerId);
        }

        void ExecuteFace(string payload, GameObject interactor)
        {
            if (!TryParseActorTarget(payload, out var actorKey, out var targetId))
                return;

            if (!DialogueActorRegistry.TryGetActor(actorKey, out var actor))
            {
                Debug.LogWarning($"[DialogueActionRunner] Actor not found: {actorKey}");
                return;
            }

            if (TryExecuteDirectionalFace(actor, targetId))
                return;

            if (string.Equals(targetId, "player", System.StringComparison.OrdinalIgnoreCase))
            {
                var player = interactor != null
                    ? interactor.transform
                    : FindFirstObjectByType<PlayerInventory>()?.transform;

                if (player != null)
                    actor.FacePlanarTarget(player.position);

                return;
            }

            if (DialogueActorRegistry.TryGetMarker(targetId, out var marker))
            {
                actor.FacePlanarTarget(marker.WorldPosition);
                return;
            }

            if (DialogueActorRegistry.TryGetActor(targetId, out var otherActor))
            {
                actor.FacePlanarTarget(otherActor.transform.position);
                return;
            }

            Debug.LogWarning($"[DialogueActionRunner] Face target not found: {targetId}");
        }

        void ExecuteStop(string actorKey)
        {
            if (!DialogueActorRegistry.TryGetActor(actorKey, out var actor))
            {
                Debug.LogWarning($"[DialogueActionRunner] Actor not found: {actorKey}");
                return;
            }

            actor.StopMoving();
            actor.ClearScriptedOverride();
        }

        void ExecuteSetFlag(string flagId, bool value)
        {
            if (gameProgress == null)
            {
                Debug.LogWarning("[DialogueActionRunner] GameProgress not found for setflag.");
                return;
            }

            if (string.IsNullOrWhiteSpace(flagId))
            {
                Debug.LogWarning("[DialogueActionRunner] setflag requires a flag id.");
                return;
            }

            gameProgress.SetFlag(flagId.Trim(), value);
        }

        void ExecuteUnlockEnterYin(string chapterText)
        {
            if (gameProgress == null)
                return;

            var chapter = ParseChapterNumber(chapterText, gameProgress.CurrentChapterNumber);
            gameProgress.UnlockEnterYin(chapter);
        }

        void ExecuteUnlockExitBattle(string chapterText)
        {
            if (gameProgress == null)
                return;

            var chapter = ParseChapterNumber(chapterText, gameProgress.CurrentChapterNumber);
            gameProgress.UnlockExitBattle(chapter);
        }

        static bool TryExecuteDirectionalFace(DialogueActorController actor, string targetId)
        {
            if (string.Equals(targetId, "left", System.StringComparison.OrdinalIgnoreCase))
            {
                actor.QueueFaceLeft();
                return true;
            }

            if (string.Equals(targetId, "right", System.StringComparison.OrdinalIgnoreCase))
            {
                actor.QueueFaceRight();
                return true;
            }

            return false;
        }

        static int ParseChapterNumber(string chapterText, int fallback)
        {
            if (string.IsNullOrWhiteSpace(chapterText))
                return fallback;

            return int.TryParse(chapterText.Trim(), out var chapter) ? chapter : fallback;
        }

        static bool TryParseActorTarget(string payload, out string actorKey, out string targetId)
        {
            actorKey = string.Empty;
            targetId = string.Empty;

            var arrowIndex = payload.IndexOf("->");
            var atIndex = payload.IndexOf('@');

            int separatorIndex;
            if (arrowIndex >= 0 && (atIndex < 0 || arrowIndex < atIndex))
                separatorIndex = arrowIndex;
            else if (atIndex >= 0)
                separatorIndex = atIndex;
            else
                return false;

            actorKey = payload.Substring(0, separatorIndex).Trim();
            targetId = payload.Substring(separatorIndex + (payload[separatorIndex] == '@' ? 1 : 2)).Trim();
            return !string.IsNullOrWhiteSpace(actorKey) && !string.IsNullOrWhiteSpace(targetId);
        }
    }
}

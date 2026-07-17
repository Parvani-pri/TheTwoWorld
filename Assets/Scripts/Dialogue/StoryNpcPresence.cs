using System;
using TwoWorlds.Core;
using TwoWorlds.Progress;
using UnityEngine;

namespace TwoWorlds.Dialogue
{
    /// <summary>
    /// Restores a story NPC's world position after scene reloads based on GameProgress,
    /// and dismisses the NPC when its chapter epilogue dialogue ends.
    /// </summary>
    public class StoryNpcPresence : MonoBehaviour
    {
        [SerializeField] GameProgress gameProgress;
        [SerializeField] string showAfterDialogueId = "1101";
        [SerializeField] string dismissAfterDialogueId = "1104";
        [SerializeField] string spawnMarkerId = "patient_spawn";
        [SerializeField] string presentMarkerId = "patient_consult";
        [SerializeField] string exitMarkerId = "patient_exit";

        DialogueActorController actorController;

        void Awake()
        {
            actorController = GetComponent<DialogueActorController>();

            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        void OnEnable() => GameEvents.ScriptDialogueEnded += OnScriptDialogueEnded;

        void OnDisable() => GameEvents.ScriptDialogueEnded -= OnScriptDialogueEnded;

        void Start() => ApplyPresence();

        void OnScriptDialogueEnded(DialogueEndInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.DialogueId) ||
                string.IsNullOrWhiteSpace(dismissAfterDialogueId))
            {
                return;
            }

            if (!string.Equals(info.DialogueId, dismissAfterDialogueId, StringComparison.OrdinalIgnoreCase))
                return;

            DismissToExit();
        }

        public void ApplyPresence()
        {
            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();

            gameObject.SetActive(true);

            if (gameProgress != null && gameProgress.HasDialogue(dismissAfterDialogueId))
            {
                SnapToMarker(exitMarkerId);
                return;
            }

            if (gameProgress != null && gameProgress.HasDialogue(showAfterDialogueId))
            {
                SnapToMarker(presentMarkerId);
                return;
            }

            SnapToMarker(spawnMarkerId);
        }

        void DismissToExit()
        {
            if (actorController == null)
                actorController = GetComponent<DialogueActorController>();

            if (actorController != null && TryResolveMarker(exitMarkerId, out var position))
            {
                actorController.MoveTo(position);
                return;
            }

            SnapToMarker(exitMarkerId);
        }

        void SnapToMarker(string markerId)
        {
            if (TryResolveMarker(markerId, out var position))
            {
                position.y = transform.position.y;
                transform.position = position;
            }
        }

        static bool TryResolveMarker(string markerId, out Vector3 position)
        {
            position = default;

            if (DialogueActorRegistry.TryGetMarker(markerId, out var marker) && marker != null)
            {
                position = marker.WorldPosition;
                return true;
            }

            var markers = FindObjectsByType<DialogueMoveMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var candidate in markers)
            {
                if (candidate == null)
                    continue;

                if (!string.Equals(candidate.MarkerId, markerId, StringComparison.OrdinalIgnoreCase))
                    continue;

                position = candidate.WorldPosition;
                return true;
            }

            return false;
        }
    }
}

using System;
using TwoWorlds.Core;
using TwoWorlds.Progress;
using UnityEngine;

namespace TwoWorlds.Dialogue
{
    [Serializable]
    public class DialogueFlagBinding
    {
        [Tooltip("When this script dialogue completes, set the flag below.")]
        public string dialogueId;

        public string setFlag;

        [Tooltip("If true, only set the flag the first time this dialogue completes.")]
        public bool onlyOnce = true;
    }

    /// <summary>
    /// Optional helper: records story flags when dialogues finish.
    /// Use this for progress tracking, not as the only unlock gate for the next stage.
    /// </summary>
    public class DialogueProgressFlagBinder : MonoBehaviour
    {
        [SerializeField] GameProgress gameProgress;
        [SerializeField] DialogueFlagBinding[] bindings;

        void Awake()
        {
            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        void OnEnable() => GameEvents.ScriptDialogueEnded += OnScriptDialogueEnded;

        void OnDisable() => GameEvents.ScriptDialogueEnded -= OnScriptDialogueEnded;

        void OnScriptDialogueEnded(DialogueEndInfo info)
        {
            if (gameProgress == null || bindings == null || string.IsNullOrWhiteSpace(info.DialogueId))
                return;

            foreach (var binding in bindings)
            {
                if (binding == null ||
                    string.IsNullOrWhiteSpace(binding.dialogueId) ||
                    string.IsNullOrWhiteSpace(binding.setFlag))
                    continue;

                if (!string.Equals(binding.dialogueId, info.DialogueId, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (binding.onlyOnce && gameProgress.HasFlag(binding.setFlag))
                    continue;

                gameProgress.SetFlag(binding.setFlag, true);
            }
        }
    }
}

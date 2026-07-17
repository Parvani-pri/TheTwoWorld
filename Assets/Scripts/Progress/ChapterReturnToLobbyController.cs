using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Progress
{
    /// <summary>
    /// After post-battle dialogue ends, unlocks epilogue and returns to MainLobby.
    /// </summary>
    public class ChapterReturnToLobbyController : MonoBehaviour
    {
        [SerializeField] GameProgress gameProgress;

        void Awake()
        {
            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        void OnEnable() => GameEvents.ScriptDialogueEnded += OnScriptDialogueEnded;

        void OnDisable() => GameEvents.ScriptDialogueEnded -= OnScriptDialogueEnded;

        void OnScriptDialogueEnded(DialogueEndInfo info)
        {
            if (gameProgress == null || string.IsNullOrWhiteSpace(info.DialogueId))
                return;

            if (!ChapterProgressCatalog.TryParseDialogueId(info.DialogueId, out var chapter, out var segment))
                return;

            if (segment != ChapterSegment.PostBattle || chapter < 1)
                return;

            gameProgress.UnlockExitBattle(chapter);
            SceneFlow.LoadMainLobby();
        }
    }
}

using TwoWorlds.Combat;
using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Progress
{
    /// <summary>
    /// Keeps chapter/segment flags in sync when scripted dialogues or combat finish.
    /// </summary>
    public class ChapterProgressController : MonoBehaviour
    {
        [SerializeField] GameProgress gameProgress;
        [SerializeField] bool autoUnlockEnterYinOnPreBattleEnd;
        [SerializeField] bool autoUnlockExitBattleOnPostBattleEnd;

        void Awake()
        {
            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        void OnEnable()
        {
            GameEvents.ScriptDialogueEnded += OnScriptDialogueEnded;
            GameEvents.CombatEnded += OnCombatEnded;
        }

        void OnDisable()
        {
            GameEvents.ScriptDialogueEnded -= OnScriptDialogueEnded;
            GameEvents.CombatEnded -= OnCombatEnded;
        }

        void OnScriptDialogueEnded(DialogueEndInfo info)
        {
            if (gameProgress == null || string.IsNullOrWhiteSpace(info.DialogueId))
                return;

            if (!ChapterProgressCatalog.TryParseDialogueId(info.DialogueId, out var chapter, out var segment))
                return;

            gameProgress.CompleteChapterSegment(chapter, segment);

            if (autoUnlockEnterYinOnPreBattleEnd && segment == ChapterSegment.PreBattle && chapter >= 1)
                gameProgress.UnlockEnterYin(chapter);

            if (autoUnlockExitBattleOnPostBattleEnd && segment == ChapterSegment.PostBattle && chapter >= 1)
                gameProgress.UnlockExitBattle(chapter);
        }

        void OnCombatEnded(CombatResult result)
        {
            if (gameProgress == null || result != CombatResult.Victory)
                return;

            var chapter = gameProgress.CurrentChapterNumber;
            if (chapter < 1)
                return;

            gameProgress.MarkCombatDone(chapter);
        }
    }
}

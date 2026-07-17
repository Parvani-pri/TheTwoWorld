using System.Collections.Generic;
using System.Text;
using TwoWorlds.AI;
using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Progress
{
    public class GameProgress : MonoBehaviour
    {
        public static GameProgress Instance { get; private set; }

        [Header("Unlock Rules")]
        [SerializeField] string xiaomeiIntroDialogueId = "1001";
        [SerializeField] string xiaomeiUnlockFlag = "xiaomei_unlocked";

        [Header("Default Chapter (optional)")]
        [SerializeField] string defaultChapterId = "ch1_grave";
        [SerializeField] string defaultChapterLabel = "第一章 · 祖坟";
        [SerializeField] string defaultStageLabel = "调查祖坟";

        readonly HashSet<string> completedDialogueIds = new();
        readonly List<string> progressNotes = new();
        readonly Dictionary<string, bool> storyFlags = new();

        string currentChapterId;
        string currentChapterLabel;
        string currentStageLabel;
        string lastCompletedDialogueId;

        int currentChapterNumber = ChapterProgressCatalog.PrologueChapter;
        ChapterSegment currentSegment = ChapterSegment.None;

        public string CurrentChapterId => currentChapterId;
        public string CurrentChapterLabel => currentChapterLabel;
        public string CurrentStageLabel => currentStageLabel;
        public string LastCompletedDialogueId => lastCompletedDialogueId;
        public int CurrentChapterNumber => currentChapterNumber;
        public ChapterSegment CurrentSegment => currentSegment;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentChapterId = defaultChapterId;
            currentChapterLabel = defaultChapterLabel;
            currentStageLabel = defaultStageLabel;
            SyncChapterFlags();
        }

        void OnEnable() => GameEvents.ScriptDialogueEnded += OnScriptDialogueEnded;

        void OnDisable()
        {
            GameEvents.ScriptDialogueEnded -= OnScriptDialogueEnded;

            if (Instance == this)
                Instance = null;
        }

        void OnScriptDialogueEnded(DialogueEndInfo info)
        {
            if (!info.HasInteractor || string.IsNullOrWhiteSpace(info.DialogueId))
                return;

            MarkDialogueCompleted(info.DialogueId, info.ProgressNote);
        }

        public void SetChapter(string chapterId, string chapterLabel, string stageLabel)
        {
            if (!string.IsNullOrWhiteSpace(chapterId))
                currentChapterId = chapterId.Trim();

            if (!string.IsNullOrWhiteSpace(chapterLabel))
                currentChapterLabel = chapterLabel.Trim();

            if (!string.IsNullOrWhiteSpace(stageLabel))
                currentStageLabel = stageLabel.Trim();
        }

        public void SetStageLabel(string stageLabel)
        {
            if (!string.IsNullOrWhiteSpace(stageLabel))
                currentStageLabel = stageLabel.Trim();
        }

        public void SetChapterProgress(int chapterNumber, ChapterSegment segment)
        {
            currentChapterNumber = Mathf.Clamp(chapterNumber, ChapterProgressCatalog.PrologueChapter, ChapterProgressCatalog.MaxChapter);
            currentSegment = segment;
            SyncChapterFlags();

            if (ChapterProgressCatalog.TryGetChapterMeta(currentChapterNumber, out var chapterId, out var chapterLabel))
            {
                currentChapterId = chapterId;
                currentChapterLabel = chapterLabel;
            }

            currentStageLabel = BuildStageLabel(currentChapterNumber, segment);
        }

        public void SetFlag(string flagId, bool value)
        {
            if (string.IsNullOrWhiteSpace(flagId))
                return;

            storyFlags[flagId.Trim()] = value;
        }

        public bool HasFlag(string flagId)
        {
            if (string.IsNullOrWhiteSpace(flagId))
                return false;

            return storyFlags.TryGetValue(flagId.Trim(), out var value) && value;
        }

        public bool HasDialogue(string dialogueId)
        {
            if (string.IsNullOrWhiteSpace(dialogueId))
                return false;

            return completedDialogueIds.Contains(dialogueId.Trim());
        }

        public void MarkDialogueCompleted(string dialogueId, string progressNote = null)
        {
            if (string.IsNullOrWhiteSpace(dialogueId))
                return;

            var id = dialogueId.Trim();
            completedDialogueIds.Add(id);
            lastCompletedDialogueId = id;

            if (!string.IsNullOrWhiteSpace(progressNote))
                AddProgressNote(progressNote.Trim());

            if (string.Equals(id, xiaomeiIntroDialogueId, System.StringComparison.OrdinalIgnoreCase))
                SetFlag(xiaomeiUnlockFlag, true);
        }

        public void AddProgressNote(string note)
        {
            if (string.IsNullOrWhiteSpace(note))
                return;

            if (progressNotes.Contains(note))
                return;

            progressNotes.Add(note);
        }

        public bool CanStartChapterDialogue(int chapter, ChapterSegment segment)
        {
            if (segment == ChapterSegment.None)
                return false;

            var dialogueId = ChapterProgressCatalog.GetDialogueId(chapter, segment);
            if (HasDialogue(dialogueId))
                return false;

            switch (segment)
            {
                case ChapterSegment.PreBattle when chapter == ChapterProgressCatalog.PrologueChapter:
                    return true;

                case ChapterSegment.PreBattle:
                    if (chapter == 1)
                        return HasDialogue(ChapterProgressCatalog.GetDialogueId(ChapterProgressCatalog.PrologueChapter, ChapterSegment.PreBattle));

                    var previousEpilogueId = ChapterProgressCatalog.GetDialogueId(chapter - 1, ChapterSegment.Epilogue);
                    return HasDialogue(previousEpilogueId);

                case ChapterSegment.EnterYin:
                    return HasFlag(ChapterProgressCatalog.GetPreBattleDoneFlag(chapter)) &&
                           HasFlag(ChapterProgressCatalog.GetEnterYinUnlockedFlag(chapter));

                case ChapterSegment.PostBattle:
                    return HasDialogue(ChapterProgressCatalog.GetDialogueId(chapter, ChapterSegment.EnterYin)) &&
                           HasFlag(ChapterProgressCatalog.GetCombatDoneFlag(chapter));

                case ChapterSegment.Epilogue:
                    return HasDialogue(ChapterProgressCatalog.GetDialogueId(chapter, ChapterSegment.PostBattle)) &&
                           HasFlag(ChapterProgressCatalog.GetExitBattleUnlockedFlag(chapter));

                default:
                    return false;
            }
        }

        public bool IsAiDialogueWindowOpen() => IsEnterYinReadinessWindowOpen();

        public bool IsEnterYinReadinessWindowOpen() => TryGetEnterYinReadinessChapter(out _);

        public bool TryGetEnterYinReadinessChapter(out int chapter)
        {
            for (var ch = 1; ch <= ChapterProgressCatalog.MaxChapter; ch++)
            {
                var preBattleId = ChapterProgressCatalog.GetDialogueId(ch, ChapterSegment.PreBattle);
                var enterYinId = ChapterProgressCatalog.GetDialogueId(ch, ChapterSegment.EnterYin);

                if (HasDialogue(preBattleId) && !HasDialogue(enterYinId))
                {
                    chapter = ch;
                    return true;
                }
            }

            chapter = 0;
            return false;
        }

        public bool HasPersistedChapterState() =>
            completedDialogueIds.Count > 0 || storyFlags.Count > 0;

        public void UnlockEnterYin(int chapter)
        {
            SetFlag(ChapterProgressCatalog.GetEnterYinUnlockedFlag(chapter), true);
            SetStageLabel($"{ChapterProgressCatalog.GetSegmentLabel(ChapterSegment.EnterYin)}准备");
        }

        public void UnlockExitBattle(int chapter)
        {
            SetFlag(ChapterProgressCatalog.GetExitBattleUnlockedFlag(chapter), true);
            SetStageLabel($"{ChapterProgressCatalog.GetSegmentLabel(ChapterSegment.Epilogue)}准备");
        }

        public void MarkCombatDone(int chapter)
        {
            SetFlag(ChapterProgressCatalog.GetCombatDoneFlag(chapter), true);
            SetStageLabel($"{ChapterProgressCatalog.GetSegmentLabel(ChapterSegment.PostBattle)}准备");
        }

        public void CompleteChapterSegment(int chapter, ChapterSegment segment)
        {
            switch (segment)
            {
                case ChapterSegment.PreBattle when chapter == ChapterProgressCatalog.PrologueChapter:
                    SetChapterProgress(1, ChapterSegment.None);
                    break;

                case ChapterSegment.PreBattle:
                    SetChapterProgress(chapter, segment);
                    SetFlag(ChapterProgressCatalog.GetPreBattleDoneFlag(chapter), true);
                    SetFlag(ChapterProgressCatalog.GetAiWindowFlag(chapter), true);
                    break;

                case ChapterSegment.EnterYin:
                    SetChapterProgress(chapter, segment);
                    SetFlag(ChapterProgressCatalog.GetAiWindowFlag(chapter), false);
                    break;

                case ChapterSegment.Epilogue when chapter < ChapterProgressCatalog.MaxChapter:
                    SetChapterProgress(chapter + 1, ChapterSegment.None);
                    break;

                default:
                    SetChapterProgress(chapter, segment);
                    break;
            }

            SyncChapterFlags();
        }

        public string BuildProgressSummary(AIContextLevel level)
        {
            if (level == AIContextLevel.None)
                return string.Empty;

            var builder = new StringBuilder();
            builder.Append("【游戏进度】");

            if (!string.IsNullOrWhiteSpace(currentChapterLabel))
                builder.Append("\n章节：").Append(currentChapterLabel);

            if (!string.IsNullOrWhiteSpace(currentChapterId))
                builder.Append("（").Append(currentChapterId).Append('）');

            if (currentChapterNumber >= 0)
            {
                builder.Append("\n章节编号：").Append(currentChapterNumber);
                builder.Append("\n章节进度：").Append(ChapterProgressCatalog.GetSegmentLabel(currentSegment));
            }

            if (!string.IsNullOrWhiteSpace(currentStageLabel))
                builder.Append("\n当前阶段：").Append(currentStageLabel);

            if (IsEnterYinReadinessWindowOpen())
                builder.Append("\n当前可确认是否入阴（战前与入阴之间）。");

            if (level == AIContextLevel.Full && completedDialogueIds.Count > 0)
            {
                builder.Append("\n已完成对话：");
                builder.Append(string.Join(", ", completedDialogueIds));
            }

            if (!string.IsNullOrWhiteSpace(lastCompletedDialogueId))
                builder.Append("\n最近对话：").Append(lastCompletedDialogueId);

            var latestNote = GetLatestProgressNote();
            if (!string.IsNullOrWhiteSpace(latestNote))
                builder.Append("\n最近进展：").Append(latestNote);

            if (level == AIContextLevel.Full)
                AppendKnownFlags(builder);

            return builder.ToString();
        }

        void AppendKnownFlags(StringBuilder builder)
        {
            var knownFacts = new List<string>();
            foreach (var pair in storyFlags)
            {
                if (pair.Value)
                    knownFacts.Add(pair.Key);
            }

            if (knownFacts.Count == 0)
                return;

            builder.Append("\n已知事实：");
            builder.Append(string.Join("；", knownFacts));
        }

        string GetLatestProgressNote() =>
            progressNotes.Count > 0 ? progressNotes[progressNotes.Count - 1] : null;

        public bool IsXiaomeiInterruptUnlocked() => HasFlag(xiaomeiUnlockFlag) || HasDialogue(xiaomeiIntroDialogueId);

        void SyncChapterFlags()
        {
            for (var chapter = 1; chapter <= ChapterProgressCatalog.MaxChapter; chapter++)
                SetFlag(ChapterProgressCatalog.GetChapterFlag(chapter), chapter == currentChapterNumber);

            if (currentChapterNumber >= 1)
                SetFlag(ChapterProgressCatalog.GetSegmentFlag(currentChapterNumber), true);

            SetFlag("story_chapter", true);
            SetFlag($"story_chapter_value_{currentChapterNumber}", true);
            SetFlag($"story_segment_value_{(int)currentSegment}", true);
        }

        static string BuildStageLabel(int chapterNumber, ChapterSegment segment)
        {
            if (chapterNumber <= ChapterProgressCatalog.PrologueChapter)
                return "序章";

            if (!ChapterProgressCatalog.TryGetChapterMeta(chapterNumber, out _, out var chapterLabel))
                return ChapterProgressCatalog.GetSegmentLabel(segment);

            if (segment == ChapterSegment.None)
                return chapterLabel;

            return $"{chapterLabel} · {ChapterProgressCatalog.GetSegmentLabel(segment)}";
        }
    }
}

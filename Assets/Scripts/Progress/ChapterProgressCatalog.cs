using System;
using System.Collections.Generic;

namespace TwoWorlds.Progress
{
    public static class ChapterProgressCatalog
    {
        public const int PrologueChapter = 0;
        public const int MaxChapter = 3;

        static readonly Dictionary<int, (string chapterId, string chapterLabel)> ChapterMeta =
            new()
            {
                [1] = ("ch1_grave", "第一章 · 祖坟"),
                [2] = ("ch2_nail", "第二章 · 镇魂钉"),
                [3] = ("ch3_fate", "第三章 · 命簿")
            };

        static readonly Dictionary<int, string> SegmentLabels = new()
        {
            [(int)ChapterSegment.PreBattle] = "战前",
            [(int)ChapterSegment.EnterYin] = "入陰",
            [(int)ChapterSegment.PostBattle] = "战后",
            [(int)ChapterSegment.Epilogue] = "尾声"
        };

        public static string GetDialogueId(int chapter, ChapterSegment segment)
        {
            if (chapter <= PrologueChapter && segment == ChapterSegment.PreBattle)
                return "1001";

            return (1000 + chapter * 100 + (int)segment).ToString();
        }

        public static bool TryParseDialogueId(string dialogueId, out int chapter, out ChapterSegment segment)
        {
            chapter = PrologueChapter;
            segment = ChapterSegment.None;

            if (string.IsNullOrWhiteSpace(dialogueId))
                return false;

            var id = dialogueId.Trim();

            if (string.Equals(id, "1001", StringComparison.OrdinalIgnoreCase))
            {
                chapter = PrologueChapter;
                segment = ChapterSegment.PreBattle;
                return true;
            }

            if (!int.TryParse(id, out var numericId) || numericId < 1101)
                return false;

            chapter = (numericId - 1000) / 100;
            var segmentValue = numericId % 100;

            if (chapter < 1 || chapter > MaxChapter)
                return false;

            if (!Enum.IsDefined(typeof(ChapterSegment), segmentValue) || segmentValue == 0)
                return false;

            segment = (ChapterSegment)segmentValue;
            return true;
        }

        public static string GetChapterFlag(int chapter) => $"story_chapter_{chapter}";

        public static string GetSegmentFlag(int chapter) => $"story_segment_{chapter}";

        public static string GetPreBattleDoneFlag(int chapter) => $"ch{chapter}_prebattle_done";

        public static string GetEnterYinUnlockedFlag(int chapter) => $"ch{chapter}_enter_yin_unlocked";

        public static string GetCombatDoneFlag(int chapter) => $"ch{chapter}_combat_done";

        public static string GetExitBattleUnlockedFlag(int chapter) => $"ch{chapter}_exit_battle_unlocked";

        public static string GetAiWindowFlag(int chapter) => $"ch{chapter}_ai_window";

        public static bool TryGetChapterMeta(int chapter, out string chapterId, out string chapterLabel)
        {
            if (ChapterMeta.TryGetValue(chapter, out var meta))
            {
                chapterId = meta.chapterId;
                chapterLabel = meta.chapterLabel;
                return true;
            }

            chapterId = string.Empty;
            chapterLabel = string.Empty;
            return false;
        }

        public static string GetSegmentLabel(ChapterSegment segment) =>
            SegmentLabels.TryGetValue((int)segment, out var label) ? label : segment.ToString();
    }
}

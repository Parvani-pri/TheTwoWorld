using System.Collections.Generic;
using System.Text;
using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Progress
{
    public class GameProgress : MonoBehaviour
    {
        public static GameProgress Instance { get; private set; }

        [Header("Unlock Rules")]
        [SerializeField] string xiaomeiIntroDialogueId = "xiaomei_intro";
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

        public string CurrentChapterId => currentChapterId;
        public string CurrentChapterLabel => currentChapterLabel;
        public string CurrentStageLabel => currentStageLabel;
        public string LastCompletedDialogueId => lastCompletedDialogueId;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            currentChapterId = defaultChapterId;
            currentChapterLabel = defaultChapterLabel;
            currentStageLabel = defaultStageLabel;
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

            if (!string.IsNullOrWhiteSpace(currentStageLabel))
                builder.Append("\n当前阶段：").Append(currentStageLabel);

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
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using TwoWorlds.Inventory;
using UnityEngine;

namespace TwoWorlds.Dialogue
{
    public static class DialogueCsvParser
    {
        const string HeaderDialogueId = "dialogue_id";
        const string HeaderSpeaker = "speaker";
        const string HeaderText = "text";
        const string HeaderPlayOnce = "play_once";
        const string HeaderRewardItemId = "reward_item_id";
        const string HeaderRewardAmount = "reward_amount";
        const string HeaderProgressNote = "progress_note";

        public static Dictionary<string, DialogueSessionData> Parse(
            string csvText,
            IEnumerable<ItemData> rewardItemCatalog = null,
            CharacterPortraitDatabase portraitDatabase = null)
        {
            var result = new Dictionary<string, DialogueSessionData>();
            if (string.IsNullOrWhiteSpace(csvText))
                return result;

            var itemLookup = BuildItemLookup(rewardItemCatalog);
            var rows = ParseRows(csvText);
            if (rows.Count == 0)
                return result;

            var header = rows[0];
            var columnIndex = BuildColumnIndex(header);

            if (!columnIndex.ContainsKey(HeaderDialogueId) ||
                !columnIndex.ContainsKey(HeaderSpeaker) ||
                !columnIndex.ContainsKey(HeaderText))
            {
                Debug.LogError("[DialogueCsvParser] CSV must contain dialogue_id, speaker, text columns.");
                return result;
            }

            var groupedLines = new Dictionary<string, List<DialogueLine>>();
            var groupedMeta = new Dictionary<string, (bool playOnce, ItemData rewardItem, int rewardAmount, string progressNote)>();

            for (var i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row.Length == 0)
                    continue;

                var dialogueId = GetCell(row, columnIndex, HeaderDialogueId);
                var speaker = GetCell(row, columnIndex, HeaderSpeaker);
                var text = GetCell(row, columnIndex, HeaderText);

                if (string.IsNullOrWhiteSpace(dialogueId))
                    continue;

                if (!groupedLines.TryGetValue(dialogueId, out var lines))
                {
                    lines = new List<DialogueLine>();
                    groupedLines[dialogueId] = lines;
                    groupedMeta[dialogueId] = (false, null, 1, string.Empty);
                }

                var playOnce = ParseBool(GetCell(row, columnIndex, HeaderPlayOnce));
                var rewardItemId = GetCell(row, columnIndex, HeaderRewardItemId);
                var rewardAmount = ParseInt(GetCell(row, columnIndex, HeaderRewardAmount), 1);
                var progressNote = GetCell(row, columnIndex, HeaderProgressNote);
                itemLookup.TryGetValue(rewardItemId, out var rewardItem);

                var meta = groupedMeta[dialogueId];
                if (playOnce)
                    meta.playOnce = true;

                if (rewardItem != null)
                {
                    meta.rewardItem = rewardItem;
                    meta.rewardAmount = rewardAmount;
                }

                if (string.IsNullOrWhiteSpace(meta.progressNote) && !string.IsNullOrWhiteSpace(progressNote))
                    meta.progressNote = progressNote;

                groupedMeta[dialogueId] = meta;

                lines.Add(new DialogueLine(speaker, text, portraitDatabase?.GetPortrait(speaker)));
            }

            foreach (var pair in groupedLines)
            {
                groupedMeta.TryGetValue(pair.Key, out var meta);
                result[pair.Key] = new DialogueSessionData(
                    pair.Key,
                    pair.Value,
                    meta.playOnce,
                    meta.rewardItem,
                    meta.rewardAmount,
                    meta.progressNote);
            }

            return result;
        }

        static Dictionary<string, ItemData> BuildItemLookup(IEnumerable<ItemData> rewardItemCatalog)
        {
            var lookup = new Dictionary<string, ItemData>(StringComparer.OrdinalIgnoreCase);
            if (rewardItemCatalog == null)
                return lookup;

            foreach (var item in rewardItemCatalog)
            {
                if (item == null)
                    continue;

                lookup[item.ItemId] = item;
                lookup[item.name] = item;
            }

            return lookup;
        }

        static Dictionary<string, int> BuildColumnIndex(IReadOnlyList<string> header)
        {
            var columnIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < header.Count; i++)
                columnIndex[header[i].Trim()] = i;

            return columnIndex;
        }

        static string GetCell(IReadOnlyList<string> row, IReadOnlyDictionary<string, int> columnIndex, string columnName)
        {
            if (!columnIndex.TryGetValue(columnName, out var index) || index >= row.Count)
                return string.Empty;

            return row[index].Trim();
        }

        static bool ParseBool(string value) =>
            bool.TryParse(value, out var result) && result;

        static int ParseInt(string value, int fallback) =>
            int.TryParse(value, out var result) ? result : fallback;

        static List<string[]> ParseRows(string csvText)
        {
            var rows = new List<string[]>();
            using var reader = new System.IO.StringReader(csvText);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                rows.Add(ParseCsvLine(line));
            }

            return rows;
        }

        static string[] ParseCsvLine(string line)
        {
            var cells = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                }

                if (c == ',' && !inQuotes)
                {
                    cells.Add(current.ToString());
                    current.Length = 0;
                    continue;
                }

                current.Append(c);
            }

            cells.Add(current.ToString());
            return cells.ToArray();
        }
    }
}

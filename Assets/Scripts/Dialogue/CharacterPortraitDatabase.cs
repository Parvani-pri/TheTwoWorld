using System;
using System.Collections.Generic;
using UnityEngine;

namespace TwoWorlds.Dialogue
{
    [Serializable]
    public struct CharacterPortraitEntry
    {
        [SerializeField] string speakerName;
        [SerializeField] Sprite portrait;

        public string SpeakerName => speakerName;
        public Sprite Portrait => portrait;
    }

    [CreateAssetMenu(fileName = "CharacterPortraitDatabase", menuName = "Two Worlds/Character Portrait Database")]
    public class CharacterPortraitDatabase : ScriptableObject
    {
        [SerializeField] CharacterPortraitEntry[] entries;

        Dictionary<string, Sprite> portraitLookup;

        public bool TryGetPortrait(string speakerName, out Sprite portrait)
        {
            portrait = null;
            if (string.IsNullOrWhiteSpace(speakerName))
                return false;

            EnsureLookup();
            return portraitLookup.TryGetValue(speakerName.Trim(), out portrait) && portrait != null;
        }

        public Sprite GetPortrait(string speakerName) =>
            TryGetPortrait(speakerName, out var portrait) ? portrait : null;

        void EnsureLookup()
        {
            if (portraitLookup != null)
                return;

            portraitLookup = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
            if (entries == null)
                return;

            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.SpeakerName) || entry.Portrait == null)
                    continue;

                portraitLookup[entry.SpeakerName.Trim()] = entry.Portrait;
            }
        }

        void OnValidate()
        {
            portraitLookup = null;
        }
    }
}

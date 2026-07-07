using System.Collections.Generic;
using TwoWorlds.Inventory;
using UnityEngine;

namespace TwoWorlds.Dialogue
{
    [CreateAssetMenu(fileName = "DialogueCsvLibrary", menuName = "Two Worlds/Dialogue CSV Library")]
    public class DialogueCsvLibrary : ScriptableObject
    {
        [SerializeField] TextAsset csvFile;
        [SerializeField] ItemData[] rewardItemCatalog;
        [SerializeField] CharacterPortraitDatabase portraitDatabase;

        Dictionary<string, DialogueSessionData> cachedDialogues;

        public TextAsset CsvFile => csvFile;

        public bool TryGetDialogue(string dialogueId, out DialogueSessionData sessionData)
        {
            sessionData = null;
            if (string.IsNullOrWhiteSpace(dialogueId))
                return false;

            EnsureCache();
            return cachedDialogues.TryGetValue(dialogueId, out sessionData);
        }

        public void Reload()
        {
            cachedDialogues = null;
            EnsureCache();
        }

        void EnsureCache()
        {
            if (cachedDialogues != null)
                return;

            cachedDialogues = csvFile != null
                ? DialogueCsvParser.Parse(csvFile.text, rewardItemCatalog, portraitDatabase)
                : new Dictionary<string, DialogueSessionData>();
        }

        void OnValidate()
        {
            cachedDialogues = null;
        }
    }
}

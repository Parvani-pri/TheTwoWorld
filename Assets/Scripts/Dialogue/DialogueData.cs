using System;
using UnityEngine;

namespace TwoWorlds.Dialogue
{
    [Serializable]
    public class DialogueLine
    {
        [SerializeField] string speakerName;
        [TextArea(2, 5)]
        [SerializeField] string text;
        [SerializeField] Sprite portrait;

        public DialogueLine()
        {
        }

        public DialogueLine(string speaker, string lineText, Sprite linePortrait = null)
        {
            speakerName = speaker;
            text = lineText;
            portrait = linePortrait;
        }

        public string SpeakerName => speakerName;
        public string Text => text;
        public Sprite Portrait => portrait;
    }

    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Two Worlds/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [SerializeField] DialogueLine[] lines;
        [SerializeField] bool playOnce;
        [SerializeField] Inventory.ItemData rewardItem;
        [SerializeField] int rewardAmount = 1;
        [TextArea(1, 3)]
        [SerializeField] string progressNote;

        public DialogueLine[] Lines => lines;
        public bool PlayOnce => playOnce;
        public Inventory.ItemData RewardItem => rewardItem;
        public int RewardAmount => rewardAmount;
        public string ProgressNote => progressNote;

        public void ApplySessionData(DialogueSessionData sessionData)
        {
            if (sessionData == null)
                return;

            lines = new DialogueLine[sessionData.Lines.Count];
            for (var i = 0; i < sessionData.Lines.Count; i++)
                lines[i] = sessionData.Lines[i];

            playOnce = sessionData.PlayOnce;
            rewardItem = sessionData.RewardItem;
            rewardAmount = sessionData.RewardAmount;
            progressNote = sessionData.ProgressNote;
        }
    }
}

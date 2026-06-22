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

        public DialogueLine[] Lines => lines;
        public bool PlayOnce => playOnce;
        public Inventory.ItemData RewardItem => rewardItem;
        public int RewardAmount => rewardAmount;
    }
}

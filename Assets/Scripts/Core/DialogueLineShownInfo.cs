using TwoWorlds.Dialogue;
using UnityEngine;

namespace TwoWorlds.Core
{
    public readonly struct DialogueLineShownInfo
    {
        public string DialogueId { get; }
        public int LineIndex { get; }
        public DialogueLine Line { get; }
        public GameObject Interactor { get; }

        public DialogueLineShownInfo(
            string dialogueId,
            int lineIndex,
            DialogueLine line,
            GameObject interactor)
        {
            DialogueId = dialogueId ?? string.Empty;
            LineIndex = lineIndex;
            Line = line;
            Interactor = interactor;
        }

        public bool HasLine => Line != null;
        public string Action => Line?.Action ?? string.Empty;
    }
}

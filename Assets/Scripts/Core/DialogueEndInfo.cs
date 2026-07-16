using UnityEngine;

namespace TwoWorlds.Core
{
    public readonly struct DialogueEndInfo
    {
        public GameObject Interactor { get; }
        public string DialogueId { get; }
        public string ProgressNote { get; }

        public DialogueEndInfo(GameObject interactor, string dialogueId, string progressNote = null)
        {
            Interactor = interactor;
            DialogueId = dialogueId ?? string.Empty;
            ProgressNote = progressNote ?? string.Empty;
        }

        public bool HasInteractor => Interactor != null;
        public bool HasProgressNote => !string.IsNullOrWhiteSpace(ProgressNote);
    }
}

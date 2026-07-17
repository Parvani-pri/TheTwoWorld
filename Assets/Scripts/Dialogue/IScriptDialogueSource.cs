using UnityEngine;

namespace TwoWorlds.Dialogue
{
    public interface IScriptDialogueSource
    {
        bool IsDialogueAvailable(GameObject interactor);
        void TriggerDialogue(GameObject interactor);
        string GetPromptText();
    }
}

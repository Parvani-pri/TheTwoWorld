using UnityEngine;

namespace TwoWorlds.Core
{
    public interface IInteractable
    {
        bool CanInteract(GameObject interactor);
        void Interact(GameObject interactor);
        string GetPromptText();
    }
}

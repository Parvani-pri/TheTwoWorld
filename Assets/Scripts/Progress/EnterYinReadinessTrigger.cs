using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Progress
{
    [RequireComponent(typeof(Collider2D))]
    public class EnterYinReadinessTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] EnterYinReadinessSession readinessSession;
        [SerializeField] string promptText = "入阴";

        void Awake()
        {
            if (readinessSession == null)
                readinessSession = EnterYinReadinessSession.FindInstance();
        }

        public bool CanInteract(GameObject interactor) => IsAvailable();

        public void Interact(GameObject interactor)
        {
            if (!IsAvailable())
                return;

            readinessSession?.ShowPrompt();
        }

        public string GetPromptText() => promptText;

        bool IsAvailable()
        {
            if (readinessSession == null)
                readinessSession = EnterYinReadinessSession.FindInstance();

            return readinessSession != null && readinessSession.IsAvailable();
        }
    }
}

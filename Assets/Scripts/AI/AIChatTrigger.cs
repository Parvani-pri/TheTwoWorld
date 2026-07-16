using TwoWorlds.Core;
using TwoWorlds.Inventory;
using UnityEngine;

namespace TwoWorlds.AI
{
    [RequireComponent(typeof(Collider2D))]
    public class AIChatTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] AIChatSession chatSession;
        [SerializeField] bool requireInteractHub;
        [SerializeField] string npcName = "小梅";
        [TextArea(3, 6)]
        [SerializeField] string npcPersona =
            "你是汉代阴阳师的助手，性格温和，说话简短，熟悉地府与傩面相关传说。";
        [SerializeField] Sprite portrait;
        [TextArea(2, 4)]
        [SerializeField] string openingMessage = "有什么想问的吗？选一个话题吧。";
        [SerializeField] string promptText = "问小梅";
        [SerializeField] int maxQuestionsPerSession = 3;
        [SerializeField] string[] quickQuestions =
        {
            "这是什么地方？",
            "我该怎么走？",
            "我背包里有什么重要的？"
        };

        public string NpcName => npcName;
        public string NpcPersona => npcPersona;
        public Sprite Portrait => portrait;
        public string OpeningMessage => openingMessage;
        public int MaxQuestionsPerSession => maxQuestionsPerSession;
        public string[] QuickQuestions => quickQuestions;

        public bool CanInteract(GameObject interactor)
        {
            if (requireInteractHub)
                return false;

            return IsAIChatAvailable(interactor);
        }

        public bool IsAIChatAvailable(GameObject interactor)
        {
            if (chatSession == null)
                chatSession = FindFirstObjectByType<AIChatSession>();

            return chatSession != null && !chatSession.IsActive;
        }

        public void Interact(GameObject interactor) => TriggerAIChat(interactor);

        public void TriggerAIChat(GameObject interactor, string openingMessageOverride = null)
        {
            if (chatSession == null)
                chatSession = FindFirstObjectByType<AIChatSession>();

            if (chatSession == null)
            {
                Debug.LogError("[AIChatTrigger] AIChatSession is missing.");
                return;
            }

            var inventory = interactor.GetComponent<PlayerInventory>();
            chatSession.StartSession(this, inventory, openingMessageOverride);
        }

        public string GetPromptText() => promptText;
    }
}

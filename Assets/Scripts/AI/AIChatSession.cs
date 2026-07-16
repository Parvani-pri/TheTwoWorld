using TwoWorlds.Core;
using TwoWorlds.Inventory;
using UnityEngine;

namespace TwoWorlds.AI
{
    public class AIChatSession : MonoBehaviour
    {
        [SerializeField] AIService aiService;
        [SerializeField] AIChatUI chatUI;

        AIChatTrigger currentTrigger;
        PlayerInventory currentInventory;
        int questionsAsked;
        int maxQuestions;
        int activeRequestId;
        bool isActive;

        public bool IsActive => isActive;

        public static AIChatSession FindInstance()
        {
            if (Instance != null)
                return Instance;

            return FindFirstObjectByType<AIChatSession>();
        }

        void Awake()
        {
            if (aiService == null)
                aiService = AIService.Instance ?? FindFirstObjectByType<AIService>();

            if (chatUI == null)
                chatUI = FindFirstObjectByType<AIChatUI>(FindObjectsInactive.Include);
        }

        void OnEnable()
        {
            if (chatUI == null)
                return;

            chatUI.QuestionSelected += OnQuestionSelected;
            chatUI.CloseRequested += EndSession;
        }

        void OnDisable()
        {
            if (chatUI == null)
                return;

            chatUI.QuestionSelected -= OnQuestionSelected;
            chatUI.CloseRequested -= EndSession;

            if (isActive)
                EndSession();
        }

        public void StartSession(AIChatTrigger trigger, PlayerInventory inventory, string openingMessageOverride = null)
        {
            if (trigger == null || isActive)
                return;

            if (chatUI == null)
            {
                Debug.LogError("[AIChatSession] AIChatUI is missing.");
                return;
            }

            currentTrigger = trigger;
            currentInventory = inventory;
            questionsAsked = 0;
            maxQuestions = Mathf.Max(1, trigger.MaxQuestionsPerSession);
            activeRequestId++;
            isActive = true;

            var openingMessage = string.IsNullOrWhiteSpace(openingMessageOverride)
                ? trigger.OpeningMessage
                : openingMessageOverride;

            GameEvents.RaiseDialogueStarted();
            chatUI.Show(
                trigger.NpcName,
                trigger.Portrait,
                trigger.QuickQuestions,
                openingMessage);
            RefreshQuestionState(setButtonsInteractable: true);
        }

        public void EndSession()
        {
            if (!isActive)
                return;

            isActive = false;
            activeRequestId++;
            currentTrigger = null;
            currentInventory = null;
            questionsAsked = 0;
            maxQuestions = 0;

            chatUI?.Hide();
            GameEvents.RaiseDialogueEnded();
        }

        void OnQuestionSelected(string question)
        {
            if (!isActive || currentTrigger == null)
                return;

            if (questionsAsked >= maxQuestions)
            {
                RefreshQuestionState(setButtonsInteractable: false);
                chatUI?.ShowReply("今日已问够，请下次再来。");
                return;
            }

            if (aiService == null)
            {
                chatUI?.ShowError("AI 服务不可用。");
                return;
            }

            var requestId = ++activeRequestId;
            chatUI.ShowLoading();

            aiService.AskWithInventoryContext(
                currentInventory,
                currentTrigger.NpcName,
                currentTrigger.NpcPersona,
                question,
                response => HandleReply(requestId, response),
                error => HandleError(requestId, error));
        }

        void HandleReply(int requestId, string response)
        {
            if (!isActive || requestId != activeRequestId)
                return;

            questionsAsked++;
            chatUI.ShowReply(response);
            RefreshQuestionState(setButtonsInteractable: questionsAsked < maxQuestions);
        }

        void HandleError(int requestId, string error)
        {
            if (!isActive || requestId != activeRequestId)
                return;

            chatUI.ShowError(error);
            RefreshQuestionState(setButtonsInteractable: questionsAsked < maxQuestions);
        }

        void RefreshQuestionState(bool setButtonsInteractable)
        {
            if (chatUI == null)
                return;

            var remaining = Mathf.Max(0, maxQuestions - questionsAsked);
            chatUI.UpdateRemainingQuestions(remaining, maxQuestions);
            chatUI.SetQuickQuestionsInteractable(setButtonsInteractable);
        }
    }
}

using TMPro;
using TwoWorlds.AI;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.Inventory
{
    public class ItemInterpretUI : MonoBehaviour
    {
        [SerializeField] AIService aiService;
        [SerializeField] Button interpretButton;
        [SerializeField] TMP_Text aiInterpretText;

        ItemData boundItem;
        int activeRequestId;

        void Awake()
        {
            if (interpretButton != null)
                interpretButton.onClick.AddListener(OnInterpretClicked);
        }

        void Start()
        {
            if (aiService == null)
                aiService = AIService.Instance ?? FindFirstObjectByType<AIService>();

            Clear();
        }

        void OnDestroy()
        {
            if (interpretButton != null)
                interpretButton.onClick.RemoveListener(OnInterpretClicked);
        }

        public void BindItem(ItemData item)
        {
            boundItem = item;
            activeRequestId++;
            SetInterpretText(string.Empty);
            SetButtonInteractable(item != null);
        }

        public void Clear()
        {
            boundItem = null;
            activeRequestId++;
            SetInterpretText(string.Empty);
            SetButtonInteractable(false);
        }

        void OnInterpretClicked()
        {
            if (boundItem == null)
                return;

            RequestInterpretation(boundItem);
        }

        void RequestInterpretation(ItemData item)
        {
            if (item == null)
                return;

            if (aiService == null)
            {
                SetInterpretText("AI 服務不可用。");
                return;
            }

            boundItem = item;
            var requestId = ++activeRequestId;
            SetButtonInteractable(false);
            SetInterpretText("解讀中…");

            aiService.AskItemInterpretation(
                item,
                response => HandleInterpretResult(requestId, response),
                error => HandleInterpretError(requestId, error));
        }

        void HandleInterpretResult(int requestId, string response)
        {
            if (requestId != activeRequestId)
                return;

            SetInterpretText(response);
            SetButtonInteractable(boundItem != null);
        }

        void HandleInterpretError(int requestId, string error)
        {
            if (requestId != activeRequestId)
                return;

            SetInterpretText($"解讀失敗：{error}");
            SetButtonInteractable(boundItem != null);
        }

        void SetInterpretText(string text)
        {
            if (aiInterpretText != null)
                aiInterpretText.text = text ?? string.Empty;
        }

        void SetButtonInteractable(bool interactable)
        {
            if (interpretButton != null)
                interpretButton.interactable = interactable;
        }
    }
}

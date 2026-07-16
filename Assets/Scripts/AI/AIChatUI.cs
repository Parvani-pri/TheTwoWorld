using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.AI
{
    public class AIChatUI : MonoBehaviour
    {
        [SerializeField] GameObject panelRoot;
        [SerializeField] TMP_Text speakerNameText;
        [SerializeField] Image portraitImage;
        [SerializeField] TMP_Text bodyText;
        [SerializeField] TMP_Text hintText;
        [SerializeField] Button closeButton;
        [SerializeField] Button[] quickQuestionButtons;

        public event Action<string> QuestionSelected;
        public event Action CloseRequested;

        void Awake()
        {
            Hide();

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);
        }

        void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseClicked);

            ClearQuickQuestionListeners();
        }

        public void Show(string npcName, Sprite portrait, string[] quickQuestions, string openingMessage)
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            if (speakerNameText != null)
                speakerNameText.text = npcName ?? string.Empty;

            if (portraitImage != null)
            {
                portraitImage.enabled = portrait != null;
                portraitImage.sprite = portrait;
            }

            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(true);
                closeButton.interactable = true;
            }

            SetBodyText(openingMessage);
            BindQuickQuestions(quickQuestions);
        }

        public void Hide()
        {
            ClearQuickQuestionListeners();

            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        public void ShowLoading()
        {
            SetBodyText("思考中…");
            SetQuickQuestionsInteractable(false);
        }

        public void ShowReply(string response)
        {
            SetBodyText(response);
        }

        public void ShowError(string error)
        {
            SetBodyText($"回复失败：{error}");
        }

        public void UpdateRemainingQuestions(int remaining, int maxQuestions)
        {
            if (hintText == null)
                return;

            if (remaining <= 0)
                hintText.text = "今日已问够，请下次再来。";
            else
                hintText.text = $"还可问 {remaining} 次（共 {maxQuestions} 次）";
        }

        public void SetQuickQuestionsInteractable(bool interactable)
        {
            if (quickQuestionButtons == null)
                return;

            foreach (var button in quickQuestionButtons)
            {
                if (button != null)
                    button.interactable = interactable;
            }
        }

        void BindQuickQuestions(string[] quickQuestions)
        {
            ClearQuickQuestionListeners();

            if (quickQuestionButtons == null)
                return;

            for (var i = 0; i < quickQuestionButtons.Length; i++)
            {
                var button = quickQuestionButtons[i];
                if (button == null)
                    continue;

                var question = quickQuestions != null && i < quickQuestions.Length
                    ? quickQuestions[i]
                    : string.Empty;

                SetButtonLabel(button, question);
                button.interactable = !string.IsNullOrWhiteSpace(question);

                if (string.IsNullOrWhiteSpace(question))
                    continue;

                var capturedQuestion = question;
                button.onClick.AddListener(() => OnQuestionClicked(capturedQuestion));
            }
        }

        static void SetButtonLabel(Button button, string label)
        {
            var labelText = button.GetComponentInChildren<TMP_Text>();
            if (labelText != null)
                labelText.text = label ?? string.Empty;
        }

        void ClearQuickQuestionListeners()
        {
            if (quickQuestionButtons == null)
                return;

            foreach (var button in quickQuestionButtons)
            {
                if (button != null)
                    button.onClick.RemoveAllListeners();
            }
        }

        void OnQuestionClicked(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return;

            QuestionSelected?.Invoke(question);
        }

        void OnCloseClicked() => CloseRequested?.Invoke();

        void SetBodyText(string text)
        {
            if (bodyText != null)
                bodyText.text = text ?? string.Empty;
        }
    }
}

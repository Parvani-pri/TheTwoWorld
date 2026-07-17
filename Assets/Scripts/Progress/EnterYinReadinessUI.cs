using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.Progress
{
    public class EnterYinReadinessUI : MonoBehaviour
    {
        [SerializeField] GameObject panelRoot;
        [SerializeField] TMP_Text speakerNameText;
        [SerializeField] Image portraitImage;
        [SerializeField] TMP_Text bodyText;
        [SerializeField] Button readyButton;
        [SerializeField] Button notReadyButton;

        CanvasGroup canvasGroup;
        Action onReadyHandler;
        Action onNotReadyHandler;

        public event Action ReadySelected;
        public event Action NotReadySelected;

        void Awake()
        {
            EnsureCanvasGroup();
            BindButtons();
            SetVisible(false);
        }

        void OnEnable()
        {
            BindButtons();
        }

        public void SetHandlers(Action onReady, Action onNotReady)
        {
            onReadyHandler = onReady;
            onNotReadyHandler = onNotReady;
        }

        public void Show(string speakerName, Sprite portrait, string message, string readyLabel, string notReadyLabel)
        {
            EnsureCanvasGroup();
            BindButtons();
            SetVisible(true);

            if (speakerNameText != null)
                speakerNameText.text = speakerName ?? string.Empty;

            if (portraitImage != null)
            {
                portraitImage.enabled = portrait != null;
                portraitImage.sprite = portrait;
            }

            if (bodyText != null)
            {
                bodyText.text = message ?? string.Empty;
                bodyText.raycastTarget = false;
            }

            if (speakerNameText != null)
                speakerNameText.raycastTarget = false;

            if (portraitImage != null)
                portraitImage.raycastTarget = false;

            SetButtonLabel(readyButton, readyLabel);
            SetButtonLabel(notReadyButton, notReadyLabel);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        void EnsureCanvasGroup()
        {
            if (panelRoot == null)
                panelRoot = gameObject;

            canvasGroup = panelRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panelRoot.AddComponent<CanvasGroup>();
        }

        void SetVisible(bool visible)
        {
            EnsureCanvasGroup();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }

            if (panelRoot != null && panelRoot != gameObject)
                panelRoot.SetActive(visible);
        }

        void BindButtons()
        {
            if (readyButton != null)
            {
                readyButton.onClick.RemoveListener(OnReadyClicked);
                readyButton.onClick.AddListener(OnReadyClicked);
            }

            if (notReadyButton != null)
            {
                notReadyButton.onClick.RemoveListener(OnNotReadyClicked);
                notReadyButton.onClick.AddListener(OnNotReadyClicked);
            }
        }

        void OnReadyClicked()
        {
            if (onReadyHandler != null)
                onReadyHandler.Invoke();
            else
                ReadySelected?.Invoke();
        }

        void OnNotReadyClicked()
        {
            if (onNotReadyHandler != null)
                onNotReadyHandler.Invoke();
            else
                NotReadySelected?.Invoke();
        }

        static void SetButtonLabel(Button button, string label)
        {
            if (button == null)
                return;

            var labelText = button.GetComponentInChildren<TMP_Text>();
            if (labelText != null)
                labelText.text = label ?? string.Empty;
        }
    }
}

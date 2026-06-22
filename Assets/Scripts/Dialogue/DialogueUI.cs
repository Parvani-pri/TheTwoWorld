using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] GameObject panelRoot;
        [SerializeField] TMP_Text speakerNameText;
        [SerializeField] TMP_Text bodyText;
        [SerializeField] Image portraitImage;
        [SerializeField] GameObject continueHint;

        void Awake()
        {
            Hide();
        }

        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            if (continueHint != null)
                continueHint.SetActive(true);
        }

        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        public void SetSpeaker(string speakerName, Sprite portrait)
        {
            if (speakerNameText != null)
                speakerNameText.text = speakerName ?? string.Empty;

            if (portraitImage != null)
            {
                portraitImage.enabled = portrait != null;
                portraitImage.sprite = portrait;
            }
        }

        public void SetBodyText(string text)
        {
            if (bodyText != null)
                bodyText.text = text ?? string.Empty;
        }
    }
}

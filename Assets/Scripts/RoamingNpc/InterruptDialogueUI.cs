using System.Collections;
using TMPro;
using TwoWorlds.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.RoamingNpc
{
    public class InterruptDialogueUI : MonoBehaviour
    {
        [SerializeField] GameObject panelRoot;
        [SerializeField] TMP_Text speakerNameText;
        [SerializeField] Image portraitImage;
        [SerializeField] TMP_Text bodyText;
        [SerializeField] GameObject continueHint;
        [SerializeField] float charactersPerSecond = 45f;

        Coroutine typingRoutine;
        bool isShowing;
        bool lineFinished;

        public bool IsShowing => isShowing;
        public bool LineFinished => lineFinished;

        void Awake()
        {
            HideImmediate();
        }

        public void Show(string speakerName, Sprite portrait, string text, float typewriterCps = 0f)
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            isShowing = true;
            lineFinished = false;

            if (speakerNameText != null)
                speakerNameText.text = speakerName ?? string.Empty;

            if (portraitImage != null)
            {
                portraitImage.enabled = portrait != null;
                portraitImage.sprite = portrait;
            }

            if (continueHint != null)
                continueHint.SetActive(false);

            if (typingRoutine != null)
                StopCoroutine(typingRoutine);

            var cps = typewriterCps > 0f ? typewriterCps : charactersPerSecond;
            typingRoutine = StartCoroutine(TypeLine(text ?? string.Empty, cps));
            GameEvents.RaiseDialogueStarted();
        }

        public void Hide()
        {
            if (!isShowing)
                return;

            if (typingRoutine != null)
            {
                StopCoroutine(typingRoutine);
                typingRoutine = null;
            }

            HideImmediate();
            GameEvents.RaiseDialogueEnded();
        }

        public void CompleteLineInstantly()
        {
            if (!isShowing || lineFinished)
                return;

            if (typingRoutine != null)
            {
                StopCoroutine(typingRoutine);
                typingRoutine = null;
            }

            lineFinished = true;

            if (continueHint != null)
                continueHint.SetActive(true);
        }

        IEnumerator TypeLine(string fullText, float cps)
        {
            if (bodyText != null)
                bodyText.text = string.Empty;

            if (cps <= 0f || string.IsNullOrEmpty(fullText))
            {
                if (bodyText != null)
                    bodyText.text = fullText;

                lineFinished = true;
                if (continueHint != null)
                    continueHint.SetActive(true);

                yield break;
            }

            var visibleCount = 0;
            var delay = 1f / cps;

            while (visibleCount < fullText.Length)
            {
                visibleCount++;
                if (bodyText != null)
                    bodyText.text = fullText.Substring(0, visibleCount);

                yield return new WaitForSecondsRealtime(delay);
            }

            lineFinished = true;
            if (continueHint != null)
                continueHint.SetActive(true);
        }

        void HideImmediate()
        {
            isShowing = false;
            lineFinished = false;

            if (panelRoot != null)
                panelRoot.SetActive(false);

            if (bodyText != null)
                bodyText.text = string.Empty;

            if (continueHint != null)
                continueHint.SetActive(false);
        }
    }
}

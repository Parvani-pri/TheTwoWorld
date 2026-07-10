using System.Collections;
using TMPro;
using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Inventory
{
    public class InventoryMessageUI : MonoBehaviour
    {
        [SerializeField] TMP_Text messageText;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] float displayDuration = 2f;
        [SerializeField] float fadeDuration = 0.25f;

        Coroutine displayRoutine;

        void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            HideImmediate();
        }

        void OnEnable() => GameEvents.InventoryAddCompleted += OnInventoryAddResult;

        void OnDisable() => GameEvents.InventoryAddCompleted -= OnInventoryAddResult;

        void OnInventoryAddResult(InventoryAddResult result, ItemData item)
        {
            var message = BuildMessage(result, item);
            if (!string.IsNullOrEmpty(message))
                Show(message);
        }

        static string BuildMessage(InventoryAddResult result, ItemData item)
        {
            var itemName = item != null ? item.DisplayName : "item";

            return result.Status switch
            {
                InventoryAddStatus.Failed => "Inventory is full",
                InventoryAddStatus.Partial =>
                    $"Not enough space. Only received {result.AddedAmount}/{result.RequestedAmount} {itemName}",
                _ => null
            };
        }

        public void Show(string message)
        {
            if (messageText == null || string.IsNullOrWhiteSpace(message))
                return;

            if (displayRoutine != null)
                StopCoroutine(displayRoutine);

            messageText.text = message;
            displayRoutine = StartCoroutine(DisplayRoutine());
        }

        IEnumerator DisplayRoutine()
        {
            SetVisible(true);

            yield return Fade(1f);

            yield return new WaitForSeconds(displayDuration);

            yield return Fade(0f);

            HideImmediate();
            displayRoutine = null;
        }

        IEnumerator Fade(float targetAlpha)
        {
            if (canvasGroup == null)
                yield break;

            var startAlpha = canvasGroup.alpha;
            var elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }

        void SetVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 0f : 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            if (messageText != null)
                messageText.gameObject.SetActive(visible);
        }

        void HideImmediate()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            if (messageText != null)
                messageText.gameObject.SetActive(false);
        }
    }
}

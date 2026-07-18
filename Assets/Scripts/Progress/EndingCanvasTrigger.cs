using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Progress
{
    /// <summary>
    /// Shows the ending canvas once, after the final chapter epilogue dialogue completes.
    /// </summary>
    public class EndingCanvasTrigger : MonoBehaviour
    {
        [SerializeField] GameObject endingCanvas;
        [SerializeField] EndingFade endingFade;
        [SerializeField] int finalChapter = ChapterProgressCatalog.MaxChapter;

        void Awake()
        {
            if (endingCanvas == null)
            {
                var canvas = GameObject.Find("EndingCanvas");
                if (canvas != null)
                    endingCanvas = canvas;
            }

            if (endingFade == null && endingCanvas != null)
                endingFade = endingCanvas.GetComponent<EndingFade>();

            HideEndingCanvas();
        }

        void OnEnable() => GameEvents.ScriptDialogueEnded += OnScriptDialogueEnded;

        void OnDisable() => GameEvents.ScriptDialogueEnded -= OnScriptDialogueEnded;

        void OnScriptDialogueEnded(DialogueEndInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.DialogueId))
                return;

            var epilogueId = ChapterProgressCatalog.GetDialogueId(finalChapter, ChapterSegment.Epilogue);
            if (!string.Equals(info.DialogueId, epilogueId, System.StringComparison.OrdinalIgnoreCase))
                return;

            ShowEndingCanvas();
        }

        void HideEndingCanvas()
        {
            if (endingCanvas != null)
                endingCanvas.SetActive(false);
        }

        void ShowEndingCanvas()
        {
            if (endingCanvas == null)
                return;

            endingCanvas.SetActive(true);

            if (endingFade == null)
                endingFade = endingCanvas.GetComponent<EndingFade>();

            endingFade?.PlayEnding();
        }
    }
}

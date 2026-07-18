using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingFade : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] TextMeshProUGUI text1;
    [SerializeField] TextMeshProUGUI text2;
    [SerializeField] string returnSceneName = "MainMenu";

    bool isPlaying;

    public void PlayEnding()
    {
        if (isPlaying)
            return;

        isPlaying = true;
        ResetTextAlpha();

        text1.DOFade(1f, 1f);
        text1.DOFade(0f, 1f).SetDelay(3f).OnComplete(() =>
        {
            text2.DOFade(1f, 1f);
            text2.DOFade(0f, 1f).SetDelay(3f).OnComplete(() =>
            {
                SceneManager.LoadScene(returnSceneName);
            });
        });
    }

    void ResetTextAlpha()
    {
        if (text1 != null)
        {
            var color = text1.color;
            color.a = 0f;
            text1.color = color;
        }

        if (text2 != null)
        {
            var color = text2.color;
            color.a = 0f;
            text2.color = color;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }
}

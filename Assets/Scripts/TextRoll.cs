using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TextRoll : MonoBehaviour
{
    public TextMeshProUGUI text;
    public CanvasGroup textcanvas;

    private void OnEnable() {
        text.DOFade(1, 1f);
        text.DOFade(0, 0.5f).SetDelay(6f).OnComplete(()=>{
        //textcanvas.DOFade(0, 0.5f);
        SceneManager.LoadScene(1);
        });

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

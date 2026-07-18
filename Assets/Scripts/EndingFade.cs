using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EndingFade : MonoBehaviour
{

    public CanvasGroup _self;
    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;

    private void OnEnable() {
        //_self.DOFade(1, 1f);

        text1.DOFade(1, 1f);
        text1.DOFade(0, 1f).SetDelay(3f).OnComplete(()=>{
           text2.DOFade (1, 1f);
           text2.DOFade(0, 1f).SetDelay(3f).OnComplete(()=>{
            SceneManager.LoadScene(0);
           });

        });
        //SceneManager.LoadScene(1);
        //});

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

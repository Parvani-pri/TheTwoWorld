using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;


public class LogoFadeinOut : MonoBehaviour
{
    public Image logo;
    public CanvasGroup _self;

    private void OnEnable() {
        logo.DOFade(1, 1f);
        logo.DOFade(0, 1f).SetDelay(3f).OnComplete(()=>{
            _self.DOFade(0, 1f);
            _self.gameObject.SetActive(false);
        
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

using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class fadeout9nMainLobby : MonoBehaviour
{
    public CanvasGroup blackcanvas;


    private void OnEnable() {
        //text.DOFade(1, 0.5f);
        //text.DOFade(0, 0.5f).SetDelay(6f).OnComplete(()=>{
        blackcanvas.DOFade(0, 1f).OnComplete(()=>{
            blackcanvas.gameObject.SetActive(false);
        
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

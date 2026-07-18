using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenuManager : MonoBehaviour
{

    public GameObject textCanvas;
    public void Quit()
    {
        Application.Quit();
    }

    public void EnterGame()
    {
        textCanvas.SetActive(true);
    }
}

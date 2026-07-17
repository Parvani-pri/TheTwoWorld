using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void Quit()
    {
        Application.Quit();
    }

    public void EnterGame()
    {
        SceneManager.LoadScene("MainLobby");
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
public class MainSceneManager : MonoBehaviour
{
    public void MenuScene()
    {
       SceneManager.LoadScene(0);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

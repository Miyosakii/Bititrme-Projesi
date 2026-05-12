using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject menuUI;
    public MonoBehaviour ghostCamera;

    bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        menuUI.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ghostCamera.enabled = true;

        isPaused = false;
    }

    void Pause()
    {
        menuUI.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ghostCamera.enabled = false;

        isPaused = true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
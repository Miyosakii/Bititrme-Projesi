using UnityEngine;

public class PanelToggleManager : MonoBehaviour
{
    [SerializeField] private GameObject controlPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (controlPanel != null)
            {
                bool isActive = controlPanel.activeSelf;
                controlPanel.SetActive(!isActive);
                
                // Panel açýlýrsa oyunu durdur ve mouse'u göster
                if (!isActive)
                {
                    Time.timeScale = 0f;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                // Panel kapanýrsa oyunu devam ettir ve mouse'u gizle
                else
                {
                    Time.timeScale = 1f;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }
}
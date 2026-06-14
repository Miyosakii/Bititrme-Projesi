using UnityEngine;

public class TeamIndicatorBillboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null)
            return;

        // ⭐ DÜZELTME: Canvas rotasyonu kamera rotasyonuna eşitle
        // Böylece Canvas her zaman kameraya doğru bakacak
        transform.rotation = mainCamera.transform.rotation;
    }
}
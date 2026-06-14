using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameEndManager : MonoBehaviour
{
    [SerializeField] private Canvas victoryCanvas;
    [SerializeField] private TextMeshProUGUI victoryTMP;
    [SerializeField] private MonoBehaviour ghostCamera;
    [SerializeField] private float delayBeforeShow = 1f; // Açılmadan önceki gecikme
    [SerializeField] private float animationDuration = 0.8f; // Animasyon süresi

    // Takım ID'lerini renklerine eşleştiren sözlük
    private static readonly Dictionary<int, string> teamColorMap = new Dictionary<int, string>
    {
        { 0, "Red" },
        { 1, "Blue" },
        { 2, "Green" },
        { 3, "Yellow" }
    };

    private static GameEndManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Eğer Inspector'dan atanmamışsa otomatik bul
        if (victoryCanvas == null)
            victoryCanvas = GetComponent<Canvas>();

        if (victoryTMP == null && victoryCanvas != null)
            victoryTMP = victoryCanvas.GetComponentInChildren<TextMeshProUGUI>();

        // Başlangıçta Canvas kapalı olsun
        if (victoryCanvas != null)
            victoryCanvas.gameObject.SetActive(false);
    }

    public static void ShowVictory(int winningTeamId)
    {
        if (instance == null)
            return;

        // Oyunu duraklat
        instance.PauseGame();

        // Coroutine başlat (gecikme + animasyon)
        instance.StartCoroutine(instance.ShowVictoryWithDelay(winningTeamId));
    }

    // ⭐ YENİ: Gecikmeli Victory gösterimi
    private IEnumerator ShowVictoryWithDelay(int winningTeamId)
    {
        // Belirtilen süre bekle (Time.timeScale = 0 olduğu için WaitForSecondsRealtime kullan)
        yield return new WaitForSecondsRealtime(delayBeforeShow);

        // Victory Canvas'ını aç
        if (victoryCanvas != null)
            victoryCanvas.gameObject.SetActive(true);

        // Takım rengini al
        string teamColor = GetTeamColor(winningTeamId);
        string victoryText = $"Team {teamColor} Wins!";

        if (victoryTMP != null)
        {
            victoryTMP.text = victoryText;

            // ⭐ YENİ: Animasyon başlat (fade in + scale)
            yield return instance.StartCoroutine(instance.AnimateVictoryPanel());
        }

        Debug.Log($"<color=green>🏆 Takım {teamColor} KAZANDI!</color>");
    }

    // ⭐ YENİ: Victory paneli animasyonu (Fade In + Scale)
    private IEnumerator AnimateVictoryPanel()
    {
        CanvasGroup canvasGroup = victoryTMP.GetComponentInParent<CanvasGroup>();

        // CanvasGroup yoksa oluştur
        if (canvasGroup == null)
            canvasGroup = victoryTMP.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = victoryTMP.gameObject.AddComponent<CanvasGroup>();

        // Başlangıç değerleri
        canvasGroup.alpha = 0f;
        RectTransform rectTransform = victoryTMP.GetComponent<RectTransform>();
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        if (rectTransform != null)
            rectTransform.localScale = startScale;

        float elapsedTime = 0f;

        // Animasyon döngüsü
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // timeScale = 0 olduğu için unscaledDeltaTime kullan
            float progress = elapsedTime / animationDuration;

            // Fade in (Alpha)
            canvasGroup.alpha = Mathf.Clamp01(progress);

            // Scale (büyüme efekti)
            if (rectTransform != null)
                rectTransform.localScale = Vector3.Lerp(startScale, endScale, progress);

            yield return null;
        }

        // Final değerler
        canvasGroup.alpha = 1f;
        if (rectTransform != null)
            rectTransform.localScale = endScale;
    }

    private static string GetTeamColor(int teamId)
    {
        return teamColorMap.ContainsKey(teamId) ? teamColorMap[teamId] : $"Team {teamId}";
    }

    // ⭐ Oyunu duraklat
    private void PauseGame()
    {
        // Zamanı durdur
        Time.timeScale = 0f;

        // Fare kilidi aç
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Kamerayı devre dışı bırak
        if (ghostCamera != null)
            ghostCamera.enabled = false;
    }

    // ⭐ Oyunu devam ettir
    public static void ResumeGame()
    {
        if (instance == null)
            return;

        // Canvas'ı kapat
        if (instance.victoryCanvas != null)
            instance.victoryCanvas.gameObject.SetActive(false);

        // Zamanı başlat
        Time.timeScale = 1f;

        // Fare kilidi kapat
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Kamerayı etkinleştir
        if (instance.ghostCamera != null)
            instance.ghostCamera.enabled = true;
    }
}
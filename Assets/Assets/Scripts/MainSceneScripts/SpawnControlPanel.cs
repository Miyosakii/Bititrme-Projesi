using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpawnControlPanel : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager;
    
    // UI Referanslarý
    [SerializeField] private Slider spawnCountSlider;
    [SerializeField] private Slider rowSizeSlider;
    [SerializeField] private TextMeshProUGUI spawnCountText;
    [SerializeField] private TextMeshProUGUI rowSizeText;
    [SerializeField] private Button spawnButton;
    [SerializeField] private Button clearButton;

    void Start()
    {
        if (spawnCountSlider != null)
        {
            spawnCountSlider.minValue = 1;
            spawnCountSlider.maxValue = 100;
            spawnCountSlider.value = spawnManager.spawnCount;
            spawnCountSlider.onValueChanged.AddListener(OnSpawnCountChanged);
        }

        if (rowSizeSlider != null)
        {
            rowSizeSlider.minValue = 1;
            rowSizeSlider.maxValue = 20;
            rowSizeSlider.value = spawnManager.rowSize;
            rowSizeSlider.onValueChanged.AddListener(OnRowSizeChanged);
        }

        if (spawnButton != null)
            spawnButton.onClick.AddListener(() => spawnManager.Spawn());

        if (clearButton != null)
            clearButton.onClick.AddListener(() => spawnManager.Clear());
    }

    private void OnSpawnCountChanged(float value)
    {
        spawnManager.spawnCount = (int)value;
        if (spawnCountText != null)
            spawnCountText.text = $"Karakter Sayýsý: {(int)value}";
    }

    private void OnRowSizeChanged(float value)
    {
        spawnManager.rowSize = (int)value;
        if (rowSizeText != null)
            rowSizeText.text = $"Satýr Boyutu: {(int)value}";
    }
}
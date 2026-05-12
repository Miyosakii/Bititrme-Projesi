using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlPanelManager : MonoBehaviour
{
    [SerializeField] private Canvas controlPanelCanvas;

    // Takým 1
    [SerializeField] private SpawnManager spawnManager1;
    [SerializeField] private TMP_Dropdown characterDropdown1;
    [SerializeField] private TMP_InputField characterCountInput1;

    // Takým 2
    [SerializeField] private SpawnManager spawnManager2;
    [SerializeField] private TMP_Dropdown characterDropdown2;
    [SerializeField] private TMP_InputField characterCountInput2;

    // Tek Oluþtur Butonu
    [SerializeField] private Button createButton;

    private GameObject[] characterPrefabs;
    private bool isPanelOpen = true;

    void Start()
    {
        LoadCharacterPrefabs();
        SetupDropdowns();
        SetupCreateButton();
        
        // Canvas baþta açýk olsun
        if (controlPanelCanvas != null)
            controlPanelCanvas.enabled = true;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("Control Panel açýlý - Oyun durduruldu");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleControlPanel();
        }
    }

    /// <summary>
    /// Canvas'ý aç/kapat
    /// </summary>
    private void ToggleControlPanel()
    {
        isPanelOpen = !isPanelOpen;

        if (controlPanelCanvas != null)
            controlPanelCanvas.enabled = isPanelOpen;

        if (isPanelOpen)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Control Panel açýldý");
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("Control Panel kapatýldý");
        }
    }

    /// <summary>
    /// Resources/Characters klasöründen prefablarý yükle
    /// </summary>
    private void LoadCharacterPrefabs()
    {
        characterPrefabs = Resources.LoadAll<GameObject>("Characters");
        
        if (characterPrefabs == null || characterPrefabs.Length == 0)
        {
            Debug.LogError("Characters klasöründe prefab bulunamadý!");
        }
    }

    /// <summary>
    /// Her iki dropdown'u doldur
    /// </summary>
    private void SetupDropdowns()
    {
        FillDropdown(characterDropdown1);
        FillDropdown(characterDropdown2);
    }

    private void FillDropdown(TMP_Dropdown dropdown)
    {
        if (dropdown == null)
            return;

        dropdown.ClearOptions();
        
        foreach (var prefab in characterPrefabs)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(prefab.name));
        }

        dropdown.RefreshShownValue();
    }

    /// <summary>
    /// Oluþtur butonunu kur
    /// </summary>
    private void SetupCreateButton()
    {
        if (createButton != null)
            createButton.onClick.AddListener(OnCreateButtonClicked);
    }

    /// <summary>
    /// Oluþtur butonuna basýnca her iki takýmý oluþtur
    /// </summary>
    private void OnCreateButtonClicked()
    {
        // Takým 1 ayarlarýný oku ve oluþtur
        int team1Count = GetCharacterCountFromInput(characterCountInput1);
        int team1Index = characterDropdown1.value;
        
        if (team1Count > 0 && team1Index >= 0)
        {
            spawnManager1.prefab = characterPrefabs[team1Index];
            spawnManager1.spawnCount = team1Count;
            spawnManager1.Spawn();
            Debug.Log($"? Takým 1 oluþturuldu: {characterPrefabs[team1Index].name} x{team1Count}");
        }

        // Takým 2 ayarlarýný oku ve oluþtur
        int team2Count = GetCharacterCountFromInput(characterCountInput2);
        int team2Index = characterDropdown2.value;
        
        if (team2Count > 0 && team2Index >= 0)
        {
            spawnManager2.prefab = characterPrefabs[team2Index];
            spawnManager2.spawnCount = team2Count;
            spawnManager2.Spawn();
            Debug.Log($"? Takým 2 oluþturuldu: {characterPrefabs[team2Index].name} x{team2Count}");
        }

        // Canvas'ý kapat ve oyunu baþlat
        ToggleControlPanel();
    }

    /// <summary>
    /// InputField'dan sayýyý oku
    /// </summary>
    private int GetCharacterCountFromInput(TMP_InputField inputField)
    {
        if (inputField == null || string.IsNullOrEmpty(inputField.text))
            return 0;

        if (int.TryParse(inputField.text, out int count) && count > 0)
            return count;

        Debug.LogWarning("Geçerli bir sayý giriniz!");
        return 0;
    }
}

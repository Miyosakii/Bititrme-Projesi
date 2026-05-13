using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlPanelManager : MonoBehaviour
{
    [SerializeField] private Canvas controlPanelCanvas;

    // Takım 1
    [SerializeField] private SpawnManager spawnManager1;
    [SerializeField] private TMP_Dropdown characterDropdown1;
    [SerializeField] private TMP_InputField characterCountInput1;

    // Takım 2
    [SerializeField] private SpawnManager spawnManager2;
    [SerializeField] private TMP_Dropdown characterDropdown2;
    [SerializeField] private TMP_InputField characterCountInput2;

    // Tek Oluştur Butonu
    [SerializeField] private Button createButton;

    private GameObject[] characterPrefabs;
    private bool isPanelOpen = true;

    void Start()
    {
        LoadCharacterPrefabs();
        SetupDropdowns();
        SetupCreateButton();

        // Canvas başta açık olsun
        if (controlPanelCanvas != null)
            controlPanelCanvas.enabled = true;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Control Panel açılı - Oyun durduruldu");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleControlPanel();
        }
    }

    /// <summary>
    /// Canvas'ı aç/kapat
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
            Debug.Log("Control Panel açıldı");
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("Control Panel kapatıldı");
        }
    }

    /// <summary>
    /// Resources/Characters klasöründen prefabları yükle
    /// </summary>
    private void LoadCharacterPrefabs()
    {
        characterPrefabs = Resources.LoadAll<GameObject>("Characters");

        if (characterPrefabs == null || characterPrefabs.Length == 0)
        {
            Debug.LogError("Characters klasöründe prefab bulunamadı!");
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
    /// Oluştur butonunu kur ve inputfield değişimlerini dinle
    /// </summary>
    private void SetupCreateButton()
    {
        if (createButton != null)
            createButton.onClick.AddListener(OnCreateButtonClicked);

        // InputField değiştiğinde butonu tekrar aktif et
        if (characterCountInput1 != null)
            characterCountInput1.onValueChanged.AddListener(OnAnyInputChanged);
        if (characterCountInput2 != null)
            characterCountInput2.onValueChanged.AddListener(OnAnyInputChanged);
    }

    /// <summary>
    /// Oluştur butonuna basınca her iki takımı oluştur.
    /// Herhangi bir inputfield geçersizse buton devre dışı bırakılır ve işlem durur.
    /// </summary>
    private void OnCreateButtonClicked()
    {
        // Her iki inputfield'ı doğrula; herhangi biri geçersizse işlemi durdur
        bool input1Valid = GetCharacterCountFromInput(characterCountInput1, out int team1Count);
        bool input2Valid = GetCharacterCountFromInput(characterCountInput2, out int team2Count);

        if (!input1Valid || !input2Valid)
        {
            createButton.interactable = false;
            Debug.LogWarning("Geçersiz girdi! Lütfen her iki alana pozitif tam sayı giriniz.");
            return;
        }

        // Takım 1 ayarlarını oku ve oluştur
        int team1Index = characterDropdown1.value;

        if (team1Count > 0 && team1Index >= 0)
        {
            spawnManager1.prefab = characterPrefabs[team1Index];
            spawnManager1.spawnCount = team1Count;
            spawnManager1.Spawn();
            Debug.Log($"✓ Takım 1 oluşturuldu: {characterPrefabs[team1Index].name} x{team1Count}");
        }

        // Takım 2 ayarlarını oku ve oluştur
        int team2Index = characterDropdown2.value;

        if (team2Count > 0 && team2Index >= 0)
        {
            spawnManager2.prefab = characterPrefabs[team2Index];
            spawnManager2.spawnCount = team2Count;
            spawnManager2.Spawn();
            Debug.Log($"✓ Takım 2 oluşturuldu: {characterPrefabs[team2Index].name} x{team2Count}");
        }

        // Canvas'ı kapat ve oyunu başlat
        ToggleControlPanel();
    }

    /// <summary>
    /// InputField'dan sayıyı oku.
    /// Geçerli pozitif tam sayı girilmişse true ve değeri out parametresiyle döner;
    /// aksi hâlde false döner ve count 0 olarak set edilir.
    /// </summary>
    private bool GetCharacterCountFromInput(TMP_InputField inputField, out int count)
    {
        count = 0;

        if (inputField == null || string.IsNullOrEmpty(inputField.text))
        {
            Debug.LogWarning("InputField boş!");
            return false;
        }

        if (int.TryParse(inputField.text, out count) && count > 0)
            return true;

        Debug.LogWarning($"Geçersiz girdi: \"{inputField.text}\" — Lütfen pozitif bir tam sayı giriniz.");
        return false;
    }

    /// <summary>
    /// Herhangi bir inputfield değiştiğinde butonu tekrar aktif et,
    /// böylece kullanıcı yanlış girdiyi düzeltebilir.
    /// </summary>
    private void OnAnyInputChanged(string _)
    {
        if (createButton != null)
            createButton.interactable = true;
    }
}
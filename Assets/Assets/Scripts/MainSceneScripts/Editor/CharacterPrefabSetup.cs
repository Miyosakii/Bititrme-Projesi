using UnityEditor;
using UnityEngine;

/// <summary>
/// Karakter prefab'larýna otomatik olarak gerekli componentleri ekler
/// </summary>
public class CharacterPrefabSetup : EditorWindow
{
    private GameObject prefabToSetup;

    [MenuItem("Tools/Character Prefab Setup")]
    public static void ShowWindow()
    {
        GetWindow<CharacterPrefabSetup>("Karakter Prefab Kurulumu");
    }

    private void OnGUI()
    {
        GUILayout.Label("Karakter Prefab Kurulumu", EditorStyles.boldLabel);
        
        prefabToSetup = EditorGUILayout.ObjectField(
            "Prefab'ý Seç",
            prefabToSetup,
            typeof(GameObject),
            false
        ) as GameObject;

        GUILayout.Space(10);

        if (GUILayout.Button("Komponentleri Ekle", GUILayout.Height(40)))
        {
            if (prefabToSetup == null)
            {
                EditorUtility.DisplayDialog("Hata", "Lütfen bir prefab seçin!", "Tamam");
                return;
            }

            SetupCharacterPrefab(prefabToSetup);
        }

        GUILayout.Space(20);
        GUILayout.Label("Otomatik Kurulum Açýklamasý:", EditorStyles.boldLabel);
        GUILayout.Label(
            "Bu araç seçilen prefab'a þu komponentleri ekler:\n" +
            "- AnimationManager\n" +
            "- CharacterState\n" +
            "- CharacterAttribute\n" +
            "- AnimationController\n\n" +
            "NOT: Animator komponentinin prefab içinde olmasý gerekir!",
            EditorStyles.wordWrappedLabel
        );
    }

    private static void SetupCharacterPrefab(GameObject prefab)
    {
        // Komponentleri ekle (varsa ekleme)
        if (prefab.GetComponent<AnimationManager>() == null)
            prefab.AddComponent<AnimationManager>();

        if (prefab.GetComponent<CharacterState>() == null)
            prefab.AddComponent<CharacterState>();

        if (prefab.GetComponent<CharacterAttribute>() == null)
            prefab.AddComponent<CharacterAttribute>();

        if (prefab.GetComponent<AnimationController>() == null)
            prefab.AddComponent<AnimationController>();

        // Animator var mý kontrol et
        if (prefab.GetComponent<Animator>() == null && 
            prefab.GetComponentInChildren<Animator>() == null)
        {
            EditorUtility.DisplayDialog(
                "Uyarý",
                "Prefab'da Animator bulunamadý! Lütfen Animator komponentini ekleyin.",
                "Tamam"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Baþarýlý",
                "Karakter prefab'ý baþarýyla kuruldu!",
                "Tamam"
            );
        }

        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();
    }
}

using UnityEditor;
using UnityEngine;

public class SpawnManagerSetup : EditorWindow
{
    private SpawnManager spawnManager;

    [MenuItem("Tools/Spawn Manager Setup")]
    public static void ShowWindow()
    {
        GetWindow<SpawnManagerSetup>("Spawn Manager Ayarlarý");
    }

    private void OnGUI()
    {
        GUILayout.Label("Spawn Manager Konfigürasyonu", EditorStyles.boldLabel);
        GUILayout.Space(10);

        spawnManager = EditorGUILayout.ObjectField(
            "SpawnManager'ý Seç",
            spawnManager,
            typeof(SpawnManager),
            true
        ) as SpawnManager;

        GUILayout.Space(15);

        if (spawnManager != null)
        {
            EditorGUILayout.LabelField("Karakter Ayarlarý", EditorStyles.boldLabel);

            spawnManager.prefab = EditorGUILayout.ObjectField(
                "Karakter Prefab'ý",
                spawnManager.prefab,
                typeof(GameObject),
                false
            ) as GameObject;

            spawnManager.spawnCount = EditorGUILayout.IntSlider(
                "Spawn Sayýsý",
                spawnManager.spawnCount,
                1,
                100
            );

            spawnManager.rowSize = EditorGUILayout.IntSlider(
                "Satýr Boyutu",
                spawnManager.rowSize,
                1,
                20
            );

            spawnManager.spacing = EditorGUILayout.FloatField(
                "Aralar Mesafesi",
                spawnManager.spacing
            );

            spawnManager.stoppingDistance = EditorGUILayout.FloatField(
                "Durma Mesafesi",
                spawnManager.stoppingDistance
            );

            EditorGUILayout.HelpBox(
                "NOT: Hareket hýzý NavMesh Agent bileþeninde ayarlanýr!",
                MessageType.Info
            );

            GUILayout.Space(15);

            if (GUILayout.Button("Karakterleri Oluþtur", GUILayout.Height(50)))
            {
                spawnManager.Spawn();
                EditorUtility.DisplayDialog("Baþarýlý", "Karakterler oluþturuldu!", "Tamam");
            }

            if (GUILayout.Button("Karakterleri Temizle", GUILayout.Height(40)))
            {
                spawnManager.Clear();
                EditorUtility.DisplayDialog("Baþarýlý", "Karakterler temizlendi!", "Tamam");
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Lütfen SpawnManager'ý seçin!", MessageType.Warning);
        }
    }
}
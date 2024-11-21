#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CSVImporterWindow : EditorWindow
{
    [MenuItem("Tools/CSV Importer")]
    public static void ShowWindow()
    {
        GetWindow<CSVImporterWindow>("CSV Importer");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Import Player Data"))
        {
            string path = EditorUtility.OpenFilePanel("Select Player CSV", "", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                CSVtoScriptableObject.ConvertCSVtoPlayerData(path);
                Debug.Log("Player data imported successfully!");
            }
        }

        if (GUILayout.Button("Import Enemy Data"))
        {
            string path = EditorUtility.OpenFilePanel("Select Enemy CSV", "", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                CSVtoScriptableObject.ConvertCSVtoEnemyData(path);
                Debug.Log("Enemy data imported successfully!");
            }
        }
        
        if (GUILayout.Button("Import Weapon Data"))
        {
            string path = EditorUtility.OpenFilePanel("Select Weapon CSV", "", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                CSVtoScriptableObject.ConvertCSVtoWeaponData(path);
                Debug.Log("Weapon data imported successfully!");
            }
        }

        if (GUILayout.Button("Import Bullet Data"))
        {
            string path = EditorUtility.OpenFilePanel("Select Bullet CSV", "", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                CSVtoScriptableObject.ConvertCSVtoBulletData(path);
                Debug.Log("Bullet data imported successfully!");
            }
        }

        if (GUILayout.Button("Import LevelUp Data"))
        {
            string path = EditorUtility.OpenFilePanel("Select LevelUp CSV", "", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                CSVtoScriptableObject.ConvertCSVtoLevelUpData(path);
                Debug.Log("LevelUp data imported successfully!");
            }
        }
    }
}
#endif
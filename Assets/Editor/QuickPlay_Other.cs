using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class QuickPlay_Other
{
    [MenuItem("UwU/QuickPlay" , false , 5)]
    public static void QuickPlay_M()
    {
        File.WriteAllText(Application.persistentDataPath + "/scene.txt", EditorSceneManager.GetActiveScene().path);

        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.EnterPlaymode();
    }

    static QuickPlay_Other()
    {
        EditorApplication.playModeStateChanged += OnQuickPlay_m;
    }

    private static void OnQuickPlay_m(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (File.Exists(Application.persistentDataPath + "/scene.txt"))
            {
                string s = File.ReadAllText(Application.persistentDataPath + "/scene.txt");
                EditorSceneManager.OpenScene(s);
            }
        }
    }
}

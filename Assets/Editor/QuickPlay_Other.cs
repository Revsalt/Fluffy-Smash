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
        JsonFile js = new JsonFile() { path = EditorSceneManager.GetActiveScene().path, hasPlayed = true };
        File.WriteAllText(Application.persistentDataPath + "/scene.txt", JsonUtility.ToJson(js));

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
                JsonFile js = JsonUtility.FromJson<JsonFile>(s);
                if (js.hasPlayed)
                    EditorSceneManager.OpenScene(js.path);

                js.hasPlayed = false;
                File.WriteAllText(Application.persistentDataPath + "/scene.txt", JsonUtility.ToJson(js));
            }
        }
    }
}

public class JsonFile
{
    public string path = "";
    public bool hasPlayed = false;
}

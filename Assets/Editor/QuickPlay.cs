using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class QuickPlay : EditorWindow
{
    [MenuItem("UwU/QuickPlayThisScene")]
    public static void ShowWindow()
    {
        GetWindow<QuickPlay>("uwu");
    }

    static QuickPlay()
    {
        EditorApplication.playModeStateChanged += AddQuickPlay;
    }

    GameObject chara = null;
    private void OnGUI()
    {
        GUILayout.BeginVertical();

        if (GUILayout.Button("QuickPlayThisScene"))
        {
            EditorApplication.EnterPlaymode();
        }

        GUILayout.BeginHorizontal();

        GUILayout.Label("PlayCharacter ");

        chara = (GameObject)EditorGUILayout.ObjectField(chara, typeof(GameObject), true);
    }

    private void Update()
    {
        if ( NetworkManager.singleton != null && NetworkManager.singleton.gameObject.name == "QuickPlay" && !NetworkManager.singleton.isNetworkActive)
        {
            NetworkManager.singleton.playerPrefab = chara;
            //NetworkManager.singleton.StartHost();
        }
    }

    private static void AddQuickPlay(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            GameObject g = Instantiate(Resources.Load("QuickPlay") as GameObject, Vector3.zero, Quaternion.identity);
        }
    }
}

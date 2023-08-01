using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEditor;

public class QuickPlay : EditorWindow
{
    [MenuItem("UwU/QuickPlay")]
    public static void ShowWindow()
    {
        GetWindow<QuickPlay>("uwu");
    }

    bool once = false;
    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("QuickPlay UwU"))
        {
            EditorApplication.EnterPlaymode();
            once = true;
        }

        if (EditorApplication.isPlaying && once)
        {
            GameObject g = (GameObject)Instantiate(Resources.Load("QuickPlay"), Vector3.zero, Quaternion.identity);
            g.GetComponentInChildren<NetworkManager>().StartHost();
            once = false;
        }
    }
}

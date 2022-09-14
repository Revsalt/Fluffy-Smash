using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Mirror;
using Mirror.Examples.NetworkRoom;
using Mirror.FizzySteam;
using UnityEditor.Build;
using static Data.Util.KeywordDependentCollection;
using UnityEditor.Build.Reporting;

[CustomEditor(typeof(NetworkRoomManagerExt))]
public class SwitchTransports : Editor
{
    public static SwitchTransports instace;
    NetworkRoomManagerExt nrme= null; 

    bool isSteamTransport = false;

    public override void OnInspectorGUI()
    {
        instace = this;

        EditorGUILayout.LabelField("Switch Transports");

        nrme = (NetworkRoomManagerExt)target;

        isSteamTransport = !nrme.GetComponent<NetworkManagerHUD>();

        if (GUILayout.Button("Switch To Telepathy") && isSteamTransport)
        {
            nrme.gameObject.AddComponent<NetworkManagerHUD>();
            nrme.gameObject.AddComponent<TelepathyTransport>();
            nrme.GetComponent<TelepathyTransport>().port = 25565;
            nrme.SetTransport(nrme.GetComponent<TelepathyTransport>());
        }

        if (GUILayout.Button("Switch To Steam") && !isSteamTransport)
        {
            DestroyImmediate(nrme.GetComponent<NetworkManagerHUD>());
            DestroyImmediate(nrme.GetComponent<TelepathyTransport>());
            nrme.SetTransport(nrme.GetComponent<FizzySteamworks>());
        }

        DrawDefaultInspector();
    }

    public void EnableSteam()
    {
        DestroyImmediate(nrme.GetComponent<NetworkManagerHUD>());
        DestroyImmediate(nrme.GetComponent<TelepathyTransport>());
        nrme.SetTransport(nrme.GetComponent<FizzySteamworks>());
    }
}

//class MyCustomBuildProcessor : IPreprocessBuildWithReport
//{
//    public int callbackOrder { get { return 0; } }
//    public void OnPreprocessBuild(BuildReport report)
//    {
//        SwitchTransports.instace.EnableSteam();
//    }
//}

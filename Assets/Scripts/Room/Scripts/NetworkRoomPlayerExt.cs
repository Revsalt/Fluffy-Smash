using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;

public class NetworkRoomPlayerExt : NetworkRoomPlayer
{
    public Button readyBtn = null;
    public GameObject UIPanel = null;
    private NetworkRoomManagerExt room;
    [SyncVar] public Team team_m;

    private NetworkRoomManagerExt Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkRoomManagerExt.singleton as NetworkRoomManagerExt;
        }
    }

    public override void OnStartClient()
    {
        //Debug.Log($"OnStartClient {gameObject}")
    }

    public override void OnClientEnterRoom()
    {
        //Debug.Log($"OnClientEnterRoom {SceneManager.GetActiveScene().path}");
    }

    public override void OnClientExitRoom()
    {
        //Debug.Log($"OnClientExitRoom {SceneManager.GetActiveScene().path}");
    }

    public override void IndexChanged(int oldIndex, int newIndex)
    {
        //Debug.Log($"IndexChanged {newIndex}");
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        //Debug.Log($"ReadyStateChanged {newReadyState}");
    }

    public override void OnGUI()
    {
        base.OnGUI();
    }


    public void Update()
    {
        if (!NetworkRoomManager.singleton.GetComponent<NetworkManagerHUD>())
        {
            GetUserNameFromPlayer = false;
            Username = SteamFriends.GetPersonaName();
        }

        if (!readyToBegin)
        {
            readyBtn.GetComponentInChildren<Text>().text = "Ready";
        }
        else { readyBtn.GetComponentInChildren<Text>().text = "Cancel"; }

        UIPanel.gameObject.SetActive(NetworkClient.active && isLocalPlayer && NetworkManager.IsSceneActive(Room.RoomScene));

    }

    public void ReadyUp()
    {
        if (readyToBegin)
        {
            CmdChangeReadyState(false);
        }
        else
        {
            CmdChangeReadyState(true);
        }

    }
}


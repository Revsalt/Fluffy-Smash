using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

public class LobbyServerControles : NetworkBehaviour
{
    public static LobbyServerControles Instance;
    [SerializeField] Button StartGameButton = null;

    private NetworkRoomManagerExt room;
    private NetworkRoomManagerExt Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkRoomManagerExt.singleton as NetworkRoomManagerExt;
        }
    }

    void Start()
    {
        this.gameObject.SetActive(isServer);
    }

    void Update()
    {
        if (Room.allPlayersReady)
        {
            StartGameButton.gameObject.SetActive(true);
        }
        else
        {
            StartGameButton.gameObject.SetActive(false);
        }
    }

    public void HandleInputDataMap(int val)
    {
        Room.GameplayScene = room.GameScenes[val];
    }

    int currentGameMode;
    public void HandleInputDataGameMode(int val)
    {
        currentGameMode = val;
    }

    float time_round = 2;
    public void HandleInputDataRoundTime(int val)
    {
        switch (val)
        {
            case 0: time_round = 1; break;
            case 1: time_round = 2; break;
            case 2: time_round = 5; break;
            case 3: time_round = 10; break;
            case 4: time_round = 20; break;
            default: time_round = 2; break;
        }
    }

    [ServerCallback]
    public void StartGame()
    {
        string gameModeName_ = "TagGameMode";
        
        switch (currentGameMode)
        {
            case 0: gameModeName_ = "TagGameMode"; break;
            case 1: gameModeName_ = "TouchDownGameMode"; break;
            case 2: gameModeName_ = "CaptureTheFlagGameMode"; break;
            default: break;
        }

        Room.gameModeName = gameModeName_;
        (Resources.Load(gameModeName_) as GameObject).GetComponent<RoundSystem>().roundTimeInMinutes = time_round;
        SynchronizeMatchData(gameModeName_ , time_round);

        Room.ServerChangeScene(Room.GameplayScene);
    }

    [ClientRpc]
    public void SynchronizeMatchData(string modeGameName , float round_time)
    {
        Room.gameModeName = modeGameName;
        (Resources.Load(modeGameName) as GameObject).GetComponent<RoundSystem>().roundTimeInMinutes = round_time;
    }

}

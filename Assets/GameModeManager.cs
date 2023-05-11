using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class GameModeManager : NetworkBehaviour
{
    [SerializeField] private Text countdown = null;
    [SerializeField] private Text winText = null;

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
        RoundSystem rs = SetGameMode(LobbyServerControles.Instance.rsd.matchGameMode);
        rs.roundTimeInMinutes = LobbyServerControles.Instance.rsd.GetRoundTime();
        Room.GameplayScene = room.GameScenes[LobbyServerControles.Instance.rsd.matchMap];

        rs.countdown = countdown;
        rs.winText = winText;
    }

    RoundSystem SetGameMode(int gamemdoe)
    {
        RoundSystem rs = null;

        switch (gamemdoe)
        {
            case 0: rs = gameObject.AddComponent<RoundSystemTag>(); break;
            case 1: rs = gameObject.AddComponent<RoundSystemTouchDown>(); break;
        }

        return rs;
    }
}

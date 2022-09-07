using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class MatchElement : MonoBehaviour
{
    [SerializeField]
    private Text matchNameText;


    private CSteamID matchID;
    public CSteamID MatchID
    {
        set
        {
            matchNameText.text = value.ToString();
            matchID = value;
        }
    }

    public void JoinMatch()
    {
        if(SteamManager.Initialized)
        {
            SteamMatchmaking.JoinLobby(matchID);
        }
    }
}

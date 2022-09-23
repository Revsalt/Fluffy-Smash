using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabScreenPlayer : MonoBehaviour
{
    [SerializeField]
    private Text playerNameText;

    [SerializeField]
    private Text playerRoleText;

    [SerializeField]
    private Text playerPingText;


    private string playerName;
    public string PlayerName
    {
        set
        {
            playerName = value;
            playerNameText.text = playerName;
        }
    }

    private bool playerRole;
    public bool PlayerRole
    {
        set
        {
            playerRole = value;
            playerRoleText.text = playerRole ? "RedTeam" : "BlueTeam";
        }
    }

    private int playerPing;

    public int PlayerPing
    {
        set
        {
            playerPing = value;
            playerPingText.text = value.ToString();
        }
    }

}

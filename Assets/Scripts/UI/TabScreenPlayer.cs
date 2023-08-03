using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabScreenPlayer : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI playerNameText;

    [SerializeField]
    private TextMeshProUGUI playerRoleText;

    [SerializeField]
    private TextMeshProUGUI playerPingText;


    private string playerName;
    public string PlayerName
    {
        set
        {
            playerName = value;
            playerNameText.text = playerName;
        }
    }

    private Team playerRole;
    public Team PlayerRole
    {
        set
        {
            playerRole = value;
            playerRoleText.text = playerRole.teamName;
            playerRoleText.color = playerRole.teamColor;
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

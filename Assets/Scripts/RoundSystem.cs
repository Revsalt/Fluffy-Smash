using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;

public class RoundSystem : NetworkBehaviour
{
    public static RoundSystem instance;

    private Animator animator = null;
    [SerializeField] Text countdown = null;
    [SerializeField] Text winText = null;
    [SerializeField] public float roundTimeInMinutes = 2;
    public bool hasTeams = false;

    public NetworkRoomManagerExt room;
    public NetworkRoomManagerExt Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkRoomManagerExt.singleton as NetworkRoomManagerExt;
        }
    }

    private void Awake()
    {
        instance = this;
        animator = GetComponent<Animator>();

        OnSceneLoad();
    }

    public void CountdownEnded()
    {
        animator.enabled = (false);

        StartRounds();
    }

    public void PlaySound(string s)
    {
        AudioManager.instance.Play2D(s);
    }

    #region Server

    public override void OnStartServer()
    {
        NetworkRoomManager.OnServerStopped += CleanUpServer;
        NetworkRoomManager.OnServerReadied += CheckToStartRound;
    }

    public List<PlayerNetworkManager> GetAllPlayers()
    {
        return FindObjectsOfType<PlayerNetworkManager>().ToList();
    }

    [ServerCallback]
    private void OnDestroy() => CleanUpServer();

    [Server]
    private void CleanUpServer()
    {
        NetworkRoomManager.OnServerStopped -= CleanUpServer;
        NetworkRoomManager.OnServerReadied -= CheckToStartRound;
    }

    [ServerCallback]
    public void StartRounds()
    {
        RpcStartRound();

        foreach (var item in GetAllPlayers())
        {
            item.GetComponent<Health>().canInfluenceDamage = true;
        }

        OnRoundStart();
    }

    [ServerCallback]
    public virtual void RoundsEnded(string endText)
    {
        RpcEndRound(endText);

        StartCoroutine(WinDelay());

        IEnumerator WinDelay()
        {
            yield return new WaitForSeconds(2);
            Room.ServerChangeScene(Room.RoomScene);
        }
    }

    [ServerCallback]
    public virtual void RoundsEnded()
    {
        RpcEndRound("");

        Room.ServerChangeScene(Room.RoomScene);
    }

    [ClientRpc]
    public void RpcEndRound(string winScreen)
    {
        winText.text = winScreen;
    }

    [Server]
    private void CheckToStartRound(NetworkConnection conn)
    {
        StartCoroutine(AllPlayersLoaded());

        IEnumerator AllPlayersLoaded()
        {
            while (Room.allPlayersReady == false)
            {
                yield return null;
            }

            yield return new WaitForSeconds(2);

            animator.enabled = true;

            RpcStartCountdown();
        }
    }

    public virtual void OnPlayerKill(NetworkIdentity theKiller)
    {
        // on any player kill
    }

    public virtual void OnRoundStart()
    {
        // on roundStarts
    }

    public virtual void OnSceneLoad()
    {
        // on roundStarts
    }

    #endregion

    #region Client

    [ClientRpc]
    private void RpcStartCountdown()
    {
        animator.enabled = true;
        foreach (var item in FindObjectsOfType<PlayerNetworkManager>())
        {
            if (item.GetComponent<NetworkIdentity>().isLocalPlayer)
                item.GetComponent<PlayerController>().DisableMovment(true);
        }
    }

    [ClientRpc]
    private void RpcStartRound()
    {
        foreach (var item in GetAllPlayers())
        {
            if (item.GetComponent<NetworkIdentity>().isLocalPlayer)
                item.GetComponent<PlayerController>().DisableMovment(false);
        }

        StartCoroutine(RoundCountDown());
    }

    IEnumerator RoundCountDown()
    {
        countdown.transform.parent.gameObject.SetActive(true);
        for (float i = roundTimeInMinutes * 60; i > 0; i--)
        {
            TimeSpan t = TimeSpan.FromSeconds(i);
            string answer = string.Format("{1:D2}:{2:D2}",
                t.Hours,
                t.Minutes,
                t.Seconds,
                t.Milliseconds);

            countdown.text = answer;

            yield return new WaitForSeconds(1f);
        }

        RoundsEnded();
    }


    #endregion
}

[Serializable]
public class Team
{
    public static Team Red = new Team()
    {
        teamColor = Color.red,
        teamName = "Red"
    };

    public static Team Blue = new Team()
    {
        teamColor = Color.blue,
        teamName = "Blue"
    };

    public static Team Yellow = new Team()
    {
        teamColor = Color.yellow,
        teamName = "Yellow"
    };

    public static Team Green = new Team()
    {
        teamColor = Color.green,
        teamName = "Green"
    };

    public static Team None = new Team()
    {
        teamColor = Color.black,
        teamName = "None"
    };

    public static Team[] AllTeams = new Team[5] {
    Red,
    Blue,
    Green,
    Yellow,
    None
    };

    public string teamName;
    public Color teamColor;
}

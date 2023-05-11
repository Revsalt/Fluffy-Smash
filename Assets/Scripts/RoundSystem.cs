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
    [HideInInspector]public Text countdown = null;
    [HideInInspector]public Text winText = null;
    [SerializeField] public float roundTimeInMinutes = 2;

    private NetworkRoomManagerExt room;
    private NetworkRoomManagerExt Room
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
    public void RoundsEnded(string endText)
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
    public void RoundsEnded()
    {
        RpcEndRound("");

        Room.ServerChangeScene(Room.RoomScene);
    }

    [ClientRpc]
    private void RpcEndRound(string winScreen)
    {
        winText.text = winScreen;
    }

    int readyPlayers = 0;

    [Server]
    private void CheckToStartRound(NetworkConnection conn)
    {
        RpcArePlayersReady();

        Debug.Log(Room.numPlayers + " || " + readyPlayers);

        if (Room.numPlayers != readyPlayers) { return; }

        animator.enabled = true;

        RpcStartCountdown();
    }

    [ClientRpc]
    public void RpcArePlayersReady()
    {
        if (SceneManager.GetActiveScene().isLoaded)
            PlayerIsReady();

    }

    [Command(requiresAuthority = false)]
    public void PlayerIsReady()
    {
        readyPlayers++;
    }

    public virtual void OnPlayerKill(NetworkIdentity theKiller)
    {
        // on any player kill
    }

    public virtual void OnRoundStart()
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
        foreach (var item in FindObjectsOfType<PlayerNetworkManager>())
        {
            if (item.GetComponent<NetworkIdentity>().isLocalPlayer)
                item.GetComponent<PlayerController>().DisableMovment(false);
        }

        StartCoroutine(RoundCountDown());
    }

    IEnumerator RoundCountDown()
    {
        countdown.transform.parent.gameObject.SetActive(true);
        for (int i = 0; i <= roundTimeInMinutes * 60; i++)
        {
            yield return new WaitForSeconds(1f);
            TimeSpan t = TimeSpan.FromSeconds(i);
            string answer = string.Format("{1:D2}:{2:D2}",
                t.Hours,
                t.Minutes,
                t.Seconds,
                t.Milliseconds);

            countdown.text = answer;
        }

        RoundsEnded();
    }

    #endregion
}

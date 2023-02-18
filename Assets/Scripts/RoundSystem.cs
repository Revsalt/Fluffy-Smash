using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class RoundSystem : NetworkBehaviour
{
    public static RoundSystem instance;

    private Animator animator = null;
    [SerializeField] private Text countdown = null;
    [SerializeField] private Text winText = null;
    [SerializeField] private float roundTimeInMinutes = 2;

    private NetworkRoomManager room;
    private NetworkRoomManager Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkRoomManager;
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

    [Server]
    private void CheckToStartRound(NetworkConnection conn)
    {
        Debug.Log(Room.numPlayers + " || " + FindObjectsOfType<PlayerNetworkManager>().Length);

        if (Room.numPlayers != FindObjectsOfType<PlayerNetworkManager>().Length) { return; }

        animator.enabled = true;

        RpcStartCountdown();
    }

    public virtual void OnPlayerKill(NetworkIdentity theKiller)
    {
        // on any player kill
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
        for (int i = 0; i <= roundTimeInMinutes*60; i++)
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

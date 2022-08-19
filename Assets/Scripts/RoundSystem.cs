using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class RoundSystem : NetworkBehaviour
{
    [SerializeField] private Animator animator = null;
    [SerializeField] private Text countdown = null;
    [SerializeField] private Text winText = null;
    [SerializeField] private float roundTimeInMinutes = 2;

    private PlayerNetworkManager Tagger = null;

    private NetworkRoomManager room;
    private NetworkRoomManager Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkRoomManager;
        }
    }

    public void CountdownEnded()
    {
        animator.enabled = false;
    }

    #region Server

    public override void OnStartServer()
    {
        NetworkRoomManager.OnServerStopped += CleanUpServer;
        NetworkRoomManager.OnServerReadied += CheckToStartRound;
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
    public void StartRound()
    {
        RpcStartRound();

        List<PlayerNetworkManager> AllPlayers = FindObjectsOfType<PlayerNetworkManager>().ToList();
        Tagger = AllPlayers[UnityEngine.Random.Range(0, AllPlayers.Count)];
        Tagger.GetComponent<TagLogic>().isTagger = true;

        StartCoroutine(RoundCountDown());
    }

    [ServerCallback]
    public void RoundEnded() // if there is more than one last guy
    {
        if (winText.text != String.Empty)
            return;

        List<PlayerNetworkManager> AllPlayers = FindObjectsOfType<PlayerNetworkManager>().ToList();
        List<PlayerNetworkManager> TaggedPlayers = new List<PlayerNetworkManager>();
        List<PlayerNetworkManager> NotTaggedPlayers = new List<PlayerNetworkManager>();

        foreach (var item in AllPlayers) //assign the winner
        {
            if (item.GetComponent<TagLogic>().isTagger)
            {
                TaggedPlayers.Add(item);
            }
            else
            {
                NotTaggedPlayers.Add(item);
            }
        }

        //declare the winner

        if (AllPlayers.Count == TaggedPlayers.Count)
        {
            StartCoroutine(Delay(delegate { winText.text = "TAGGERS WON!"; }));
        }
        else
        {
            List<string> names = new List<string>();
            foreach (var item in NotTaggedPlayers)
            {
                names.Add(item.GetComponent<PlayerNetworkManager>().nrp.username);
            }

            StartCoroutine(Delay(delegate { winText.text = String.Join("," , names.ToArray()) + " WON!"; }));
        }

        RpcEndRound(winText.text);

        IEnumerator Delay(Action act)
        {
            act.Invoke();
            yield return new WaitForSeconds(3);
            Room.ServerChangeScene(Room.RoomScene);
        }

        foreach (var item in AllPlayers)
        {
            item.GetComponent<TagLogic>().isTagged = false;
            item.GetComponent<TagLogic>().isTagger = false;
        }
    }

    [ClientRpc]
    private void RpcEndRound(string winScreen)
    {
        winText.text = winScreen;
    }

    private void FixedUpdate()
    {
        if (!isServer)
            return;

        List<PlayerNetworkManager> AllPlayers = FindObjectsOfType<PlayerNetworkManager>().ToList();
        List<PlayerNetworkManager> TaggedPlayers = new List<PlayerNetworkManager>();

        if (AllPlayers.Count <= 1)
            return;

        foreach (var item in AllPlayers)
        {
            if (item.GetComponent<TagLogic>().isTagger)
            {
                TaggedPlayers.Add(item);
            }
        }

        if (AllPlayers.Count == TaggedPlayers.Count)
        {
            RoundEnded();
        }
    }

    [Server]
    private void CheckToStartRound(NetworkConnection conn)
    {
        Debug.Log(Room.numPlayers + " || " + FindObjectsOfType<PlayerNetworkManager>().Length);

        if (Room.numPlayers != FindObjectsOfType<PlayerNetworkManager>().Length) { return; }

        animator.enabled = true;

        RpcStartCountdown();
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

        RoundEnded();
    }

    #endregion
}

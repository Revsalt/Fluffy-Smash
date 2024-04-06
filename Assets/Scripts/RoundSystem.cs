using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RoundSystem : NetworkBehaviour
{
    [SerializeField] private Animator animator = null;

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

    [ServerCallback]
    private void Update()
    {

    }

    public override void OnStartServer()
    {
        CheckToStartRound();
    }

    public void Win(string winner)
    {

    }

    [ServerCallback]
    public void StartRound()
    {
        RpcStartRound();
    }

    [Server]
    private void CheckToStartRound()
    {
        Debug.Log(Room.numPlayers + " || " + FindObjectsOfType<Player>().Length);

        if (Room.numPlayers != FindObjectsOfType<Player>().Length) { return; }

        animator.enabled = true;

        RpcStartCountdown();
    }

    #endregion

    #region Client

    [ClientRpc]
    private void RpcStartCountdown()
    {
        animator.enabled = true;
        foreach (var item in FindObjectsOfType<Player>())
        {
            if (item.GetComponent<NetworkIdentity>().isLocalPlayer)
                item.GetComponent<Player>().DisableMovment(true);
        }
    }

    [ClientRpc]
    private void RpcStartRound()
    {
        foreach (var item in FindObjectsOfType<Player>())
        {
            if (item.GetComponent<NetworkIdentity>().isLocalPlayer)
                item.GetComponent<Player>().DisableMovment(false);
        }
    }

    #endregion
}

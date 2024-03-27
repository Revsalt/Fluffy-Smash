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
        NetworkRoomManager.OnServerStopped += CleanUpServer;
        NetworkRoomManager.OnServerReadied += CheckToStartRound;
    }

    public void Win(string winner)
    {

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
        Player[] AllPlayer = FindObjectsOfType<Player>();

        List<Player> TeamA = new List<Player>();
        List<Player> TeamB = new List<Player>();

        foreach (Player player in AllPlayer)
        {
            int TeamID = Random.Range(0, 1);

            if (TeamID == 0)
            {
                player.GetComponent<PlayerNetworkManager>().Role = "TeamRed";
                TeamA.Add(player);
            }
            else
            {
                player.GetComponent<PlayerNetworkManager>().Role = "TeamBlue";
                TeamB.Add(player);
            }
        }

        RpcStartRound();
    }

    [Server]
    private void CheckToStartRound(NetworkConnection conn)
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

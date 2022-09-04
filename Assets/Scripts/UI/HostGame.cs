using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostGame : MonoBehaviour
{
    public void HostLobby()
    {
        /*
        if(NetworkManager.singleton)
        {
            NetworkManager.singleton.GetComponent<SteamLobby>().HostLobby();
        }*/

        if(SteamLobby.Instance)
        {
            SteamLobby.Instance.HostLobby();
        }
    }
}

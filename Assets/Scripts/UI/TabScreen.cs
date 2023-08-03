using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class TabScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject TabScreenObject;

    [SerializeField]
    private TabScreenPlayer TabScreenPlayerPrefab;


    [SerializeField]
    private GameObject PlayersListObject;


    //public List<PlayerNetworkManager> 

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UpdatePlayerList();
            TabScreenObject.SetActive(true);
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            TabScreenObject.SetActive(false);
        }
    }

    void UpdatePlayerList()
    {
        List<PlayerNetworkManager> AllPlayers = FindObjectsOfType<PlayerNetworkManager>().ToList();

        foreach (Transform x in PlayersListObject.transform)
        {
            Destroy(x.gameObject);
        }

        foreach (PlayerNetworkManager newPlayer in AllPlayers)
        {
            //Taggers
            TabScreenPlayer tabScreenPlayer = Instantiate(TabScreenPlayerPrefab, PlayersListObject.transform);
            tabScreenPlayer.PlayerName = newPlayer.nrp.username;
            tabScreenPlayer.PlayerRole = newPlayer.Team_m;
            tabScreenPlayer.PlayerPing = Convert.ToInt32(newPlayer.nrp.ping);
        }
    }
}

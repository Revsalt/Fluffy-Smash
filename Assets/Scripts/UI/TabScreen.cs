using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class TabScreen : NetworkBehaviour
{
    [SerializeField]
    private GameObject TabScreenObject;

    [SerializeField]
    private TabScreenPlayer TabScreenPlayerPrefab;


    [SerializeField]
    private GameObject TaggersListObject;

    [SerializeField]
    private GameObject RunnerListObject;


    //public List<PlayerNetworkManager> 

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            TabScreenObject.SetActive(true);
        }
       
        if(Input.GetKeyUp(KeyCode.Tab))
        {
            TabScreenObject.SetActive(false);
        }


        List<PlayerNetworkManager> AllPlayers = FindObjectsOfType<PlayerNetworkManager>().ToList();


        foreach(Transform x in TaggersListObject.transform)
        {
            Destroy(x.gameObject);
        }

        foreach (Transform x in RunnerListObject.transform)
        {
            Destroy(x.gameObject);
        }

        foreach(PlayerNetworkManager newPlayer in AllPlayers)
        {
            if(newPlayer.GetComponent<Health>().TeamName == "RedTeam")
            {
                //Taggers
                TabScreenPlayer tabScreenPlayer = Instantiate(TabScreenPlayerPrefab, TaggersListObject.transform);
                tabScreenPlayer.PlayerName = newPlayer.nrp.username;
                tabScreenPlayer.PlayerRole = true;
                tabScreenPlayer.PlayerPing = Convert.ToInt32(newPlayer.nrp.ping);
            }
            else
            {
                //Runners
                TabScreenPlayer tabScreenPlayer = Instantiate(TabScreenPlayerPrefab, RunnerListObject.transform);
                tabScreenPlayer.PlayerName = newPlayer.nrp.username;
                tabScreenPlayer.PlayerRole = false;
                tabScreenPlayer.PlayerPing = Convert.ToInt32(newPlayer.nrp.ping);
            }
        }
    }
}

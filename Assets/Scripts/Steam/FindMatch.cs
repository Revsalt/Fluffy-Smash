using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class FindMatch : MonoBehaviour
{
    public MatchElement MatchElementPrefab;


    //SteamCallresults
    private CallResult<LobbyMatchList_t> lobbyMatchList;

    private void Start()
    {
        lobbyMatchList = CallResult<LobbyMatchList_t>.Create(OnFoundLobbies);

        Fetch();
    }

    void Fetch()
    {
        //Make a request 
        if(SteamManager.Initialized)
        {
            FindLobby();
        }
    }




    private void FindLobby()
    {
        if (!SteamManager.Initialized)
        {
            return;
        }

        SteamAPICall_t handle = SteamMatchmaking.RequestLobbyList();
        lobbyMatchList.Set(handle);

    }

    private void OnFoundLobbies(LobbyMatchList_t callback, bool bIOFailure)
    {
        if (bIOFailure)
        {
            Debug.Log("Failed to get match list");
            return;
        }

        //Remove all elements
        foreach (Transform t in gameObject.transform)
        {
            Destroy(t.gameObject);
        }


        for (int i = 0; i < callback.m_nLobbiesMatching; i++)
        {
            CSteamID LobbyID = SteamMatchmaking.GetLobbyByIndex(i);

            MatchElement matchElement = Instantiate(MatchElementPrefab, gameObject.transform);

            matchElement.MatchID = LobbyID;
        }


        Invoke("Fetch", 5f);
    }
}

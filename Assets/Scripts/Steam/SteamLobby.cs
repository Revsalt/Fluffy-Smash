using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamLobby : MonoBehaviour
{
    [SerializeField] private GameObject buttons = null;

    private NetworkManager networkManager;

    //Steam data keys
    private const string HostAddressKey = "HostAddress";

    //SteamCallbacks
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;


    static private SteamLobby instance;

    static public SteamLobby Instance
    {
        get
        {
            if(instance == null)
            {
                //Add this script to any gameobject to access it
                return null;
            }
            else
            {
                return instance;
            }
        }
    }

    [HideInInspector]
    public bool IsInLobby = false;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        networkManager = GetComponent<NetworkManager>();


        if(!SteamManager.Initialized)
        {
            return;
        }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);


    }

    public void HostLobby()
    {
        //buttons.SetActive(false);

        Debug.Log("Trying to create a lobby");

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, networkManager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if(callback.m_eResult != EResult.k_EResultOK)
        {
            //buttons.SetActive(true);
            IsInLobby = false;
            return;
        }

        networkManager.StartHost();

        //Set Host Address in lobby data
        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            HostAddressKey,
            SteamUser.GetSteamID().ToString());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        IsInLobby = true;

        //Don't run on server
        if (NetworkServer.active)
        {
            return;
        }

        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            HostAddressKey);

        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();

        //buttons.SetActive(false);
    }
}

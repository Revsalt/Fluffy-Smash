using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;
using Mirror.Examples.NetworkRoom;

public class RoundSystemTouchDown : NetworkBehaviour
{
    int RedTeamScore = 0,BlueTeamScore = 0;

    public static RoundSystemTouchDown instace;
    [SerializeField] private Animator animator = null;
    [SerializeField] private Text countdown = null;
    [SerializeField] private Text score = null;
    [SerializeField] private GameObject countdownPanel = null;
    [SerializeField] private GameObject Ball = null;
    [SerializeField] private Text winText = null;
    [SerializeField] private float roundTimeInMinutes = 2;
    [SerializeField] private float maxScore = 5;
    
    List<PlayerNetworkManager> TeamRed = new List<PlayerNetworkManager>();
    List<PlayerNetworkManager> TeamBlue = new List<PlayerNetworkManager>();

    private NetworkRoomManager room;
    private NetworkRoomManager Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkRoomManager;
        }
    }

    void Awake()
    {
        instace = this;
    }

    public void CountdownEnded()
    {
        countdownPanel.SetActive(false);
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
        StartCoroutine(RoundCountDown());
    }

    [ServerCallback]
    public void RoundEnded(string message) // if there is more than one last guy
    {
        if (winText.text != String.Empty)
            return;

        winText.text = message;

        StartCoroutine(Delay(delegate { }));

        RpcEndRound(winText.text);

        if (message == "RedTeam")
        {
            RedTeamScore++;
        } else {BlueTeamScore++; }
      
        UpdateGameScore(RedTeamScore + " : " + BlueTeamScore);

        IEnumerator Delay(Action act)
        {
            act.Invoke();
            yield return new WaitForSeconds(5);

            if (RedTeamScore == maxScore || BlueTeamScore == maxScore)
            {
                Room.ServerChangeScene(Room.RoomScene);
            }
            else
            {
                Reset();
            }
        }
    }

    [ClientRpc]
    void UpdateGameScore(string scoreText)
    {
        score.text = scoreText;
    }

    void Reset()
    {
        ScatterPlayers();
            
        StopAllCoroutines();

        GameObject g = Instantiate(Ball , Vector3.zero + Vector3.up , Quaternion.identity , null);
        NetworkServer.Spawn(g);

        winText.text = "";
        countdown.text = "";
        countdownPanel.SetActive(true);
        animator.enabled = true;
        RpcStartCountdown();
    }

    [ClientRpc]
    private void RpcEndRound(string winScreen)
    {
        winText.text = winScreen;
    }

    [TargetRpc]
    private void RpcSetStartPosition(NetworkConnection nctc , Vector3 pos)
    {
        nctc.identity.GetComponent<PlayerController>().SetPlayerPosition(pos);
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        foreach (var item in TeamRed)
        {
            if (item == null)
            {
                Room.ServerChangeScene(Room.RoomScene);
            }
        }

        foreach (var item in TeamBlue)
        {
            if (item == null)
            {
                Room.ServerChangeScene(Room.RoomScene);
            }
        }
    }

    [Server]
    private void CheckToStartRound(NetworkConnection conn)
    {
        Debug.Log(Room.numPlayers + " || " + FindObjectsOfType<PlayerNetworkManager>().Length);

        if (Room.numPlayers != FindObjectsOfType<PlayerNetworkManager>().Length) { return; }

        animator.enabled = true;

        InsializeTeams();

        RpcStartCountdown();
    }

    void InsializeTeams()
    {
        foreach(var item in FindObjectsOfType<PlayerNetworkManager>().ToList()) 
        {
            item.GetComponent<TagLogic>().isTagger = true;
        }

        GameObject g = Instantiate(Ball , Vector3.zero + Vector3.up , Quaternion.identity , null);
        NetworkServer.Spawn(g);

        List<PlayerNetworkManager> allPlayers = FindObjectsOfType<PlayerNetworkManager>().ToList();
        
        int redTeamCount = 0, blueTeamCount = 0;
        
        for (int i = 0; i < allPlayers.Count; i++)
        {
            float chance = UnityEngine.Random.Range(0 , 100);
            bool isRed = chance > 50;

            if (isRed && redTeamCount == Mathf.RoundToInt(allPlayers.Count / 2)) isRed = false;
            if (!isRed && blueTeamCount == Mathf.RoundToInt(allPlayers.Count / 2)) isRed = true;

            if (isRed)
            {
                TeamRed.Add(allPlayers[i]);
                allPlayers[i].GetComponent<TagLogic>().TeamName = "RedTeam";
                redTeamCount++;
            }
            else
            {
                TeamBlue.Add(allPlayers[i]);
                allPlayers[i].GetComponent<TagLogic>().TeamName = "BlueTeam";
                blueTeamCount++;
            }
        }

        ScatterPlayers();
    }

    void ScatterPlayers()
    {
        List<PlayerNetworkManager> allPlayers = FindObjectsOfType<PlayerNetworkManager>().ToList();

        for (int i = 0; i < allPlayers.Count; i++)
        {
            RpcSetStartPosition(allPlayers[i].GetComponent<NetworkIdentity>().connectionToClient ,
            NetworkStartPosition.GetSpawnPoistionRandomAtTeam(allPlayers[i].GetComponent<TagLogic>().TeamName));
            allPlayers[i].GetComponent<TagLogic>().isTagger = true;
        }
    }

    #endregion

    #region Client

    [ClientRpc]
    private void RpcStartCountdown()
    {  
        winText.text = "";
        countdown.text = "";
        countdownPanel.SetActive(true);
        animator.enabled = true;
        animator.SetTrigger("Start");

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

        RoundEnded("GameOver");
    }

    #endregion
}

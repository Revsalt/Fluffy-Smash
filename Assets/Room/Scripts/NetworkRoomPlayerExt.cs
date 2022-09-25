using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;

namespace Mirror.Examples.NetworkRoom
{
    [AddComponentMenu("")]
    public class NetworkRoomPlayerExt : NetworkRoomPlayer
    {

        public Button StartGameButton = null;
        public Button readyBtn = null;
        public GameObject UIPanel = null;
        private NetworkRoomManagerExt room;
        private NetworkRoomManagerExt Room
        {
            get
            {
                if (room != null) { return room; }
               return room = NetworkManager.singleton as NetworkRoomManagerExt;
            }
        }

        public override void OnStartClient()
        {
            //Debug.Log($"OnStartClient {gameObject}")
        }

        public override void OnClientEnterRoom()
        {
            //Debug.Log($"OnClientEnterRoom {SceneManager.GetActiveScene().path}");
        }

        public override void OnClientExitRoom()
        {
            //Debug.Log($"OnClientExitRoom {SceneManager.GetActiveScene().path}");
        }

        public override void IndexChanged(int oldIndex, int newIndex)
        {
            //Debug.Log($"IndexChanged {newIndex}");
        }

        public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
        {
            //Debug.Log($"ReadyStateChanged {newReadyState}");
        }

        public override void OnGUI()
        {
            base.OnGUI();
        }

        
        public void Update()
        {          
            if(!NetworkRoomManager.singleton.GetComponent<NetworkManagerHUD>())
            {
                GetUserNameFromPlayer = false;
                Username = SteamFriends.GetPersonaName();
            }

            if (Room.allPlayersReady && isServer)
            {
                StartGameButton.gameObject.SetActive(true);
            }
            else
            {
                StartGameButton.gameObject.SetActive(false);
            }

            if (!readyToBegin)
            {
                readyBtn.GetComponentInChildren<Text>().text = "Ready";
            } else {readyBtn.GetComponentInChildren<Text>().text = "Cancel"; }

            UIPanel.gameObject.SetActive(NetworkClient.active && isLocalPlayer && NetworkManager.IsSceneActive(room.RoomScene));
        }

        public void StartGame()
        {
            Room.GameplayScene = Room.GameScenes[Random.Range(0, Room.GameScenes.Length)];

            Room.ServerChangeScene(Room.GameplayScene);
        }

        
        public void ReadyUp()
        {
            if (readyToBegin)
            {
                CmdChangeReadyState(false);
            }
            else
            {
                CmdChangeReadyState(true);
            }
        
        }
    }
}

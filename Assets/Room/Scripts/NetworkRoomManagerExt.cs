using UnityEngine;

namespace Mirror.Examples.NetworkRoom
{
    [AddComponentMenu("")]
    public class NetworkRoomManagerExt : NetworkRoomManager
    {
        /// <summary>
        /// Adding any character you want
        /// </summary>
        [Tooltip("List of All your Characters")]
        public Character[] characters = default;


        [Scene]
        public string[] GameScenes = default;

        /// <summary>
        /// This is called on the server when a networked scene finishes loading.
        /// </summary>
        /// <param name="sceneName">Name of the new scene.</param>
        public override void OnRoomServerSceneChanged(string sceneName)
        {
            base.OnRoomServerSceneChanged(sceneName);
        }

        /// <summary>
        /// Called just after GamePlayer object is instantiated and just before it replaces RoomPlayer object.
        /// This is the ideal point to pass any data like player name, credentials, tokens, colors, etc.
        /// into the GamePlayer object as it is about to enter the Online scene.
        /// </summary>
        /// <param name="roomPlayer"></param>
        /// <param name="gamePlayer"></param>
        /// <returns>true unless some code in here decides it needs to abort the replacement</returns>
        public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
        {
            base.OnRoomServerSceneLoadedForPlayer(conn , roomPlayer , gamePlayer);
            //PlayerScore playerScore = gamePlayer.GetComponent<PlayerScore>();
            //playerScore.index = roomPlayer.GetComponent<NetworkRoomPlayer>().index;

            gamePlayer.GetComponent<PlayerNetworkManager>().nrp = roomPlayer.GetComponent<NetworkRoomPlayer>();

            return true;
        }

        public override void OnRoomStopClient()
        {
            base.OnRoomStopClient();
        }

        public override void SceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer)
        {
            // Debug.LogFormat(LogType.Log, "NetworkRoom SceneLoadedForPlayer scene: {0} {1}", SceneManager.GetActiveScene().path, conn);

            if (IsSceneActive(RoomScene))
            {
                // cant be ready in room, add to ready list
                PendingPlayer pending;
                pending.conn = conn;
                pending.roomPlayer = roomPlayer;
                pendingPlayers.Add(pending);
                return;
            }

            GameObject gamePlayer = OnRoomServerCreateGamePlayer(conn, roomPlayer);
            if (gamePlayer == null)
            {
                // get start position from base class
                Transform startPos = GetStartPosition();
                gamePlayer = startPos != null
                    ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                    : Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            }

            GameObject gamePlayer_ = Instantiate(characters[roomPlayer.GetComponent<NetworkRoomPlayer>().Character].GameplayCharacterPrefab, GetStartPosition().position, Quaternion.identity);
            NetworkServer.Spawn(gamePlayer_, conn);

            if (!OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer_))
                return;


            //NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, true);

            NetworkServer.ReplacePlayerForConnection(conn, gamePlayer_, true);
            GamePlayers.Add(gamePlayer);
        }

        public override void OnRoomStopServer()
        {
            base.OnRoomStopServer();
        }

        /*
            This code below is to demonstrate how to do a Start button that only appears for the Host player
            showStartButton is a local bool that's needed because OnRoomServerPlayersReady is only fired when
            all players are ready, but if a player cancels their ready state there's no callback to set it back to false
            Therefore, allPlayersReady is used in combination with showStartButton to show/hide the Start button correctly.
            Setting showStartButton false when the button is pressed hides it in the game scene since NetworkRoomManager
            is set as DontDestroyOnLoad = true.
        */

        bool showStartButton;

        public override void OnRoomServerPlayersReady()
        {
            base.OnRoomServerPlayersReady();
            // calling the base method calls ServerChangeScene as soon as all players are in Ready state.
#if UNITY_SERVER
            base.OnRoomServerPlayersReady();
#endif
        }

        public override void OnGUI()
        {
            base.OnGUI();

            if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME") && NetworkManager.IsSceneActive(RoomScene))
            {
                // set to false to hide it in the game scene
                showStartButton = false;

                GameplayScene = GameScenes[Random.Range(0, GameScenes.Length)];

                ServerChangeScene(GameplayScene);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EmptyPlayer : NetworkBehaviour
{
    [SyncVar]public NetworkRoomPlayer nrp;
    [SerializeField]private List<Character> characterInstances = new List<Character>();

    private NetworkRoomManager room;
    private NetworkRoomManager Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkRoomManager;
        }
    }

    private void Start()
    {
        if (!isLocalPlayer)
            return;

        CmdSpawnCharacter(Room.myCharacterIndex);
    }


    [Command(requiresAuthority = false)]
    public void CmdSpawnCharacter(int index, NetworkConnectionToClient sender = null)
    {

        GameObject characterInstance = Instantiate(characterInstances[index].GameplayCharacterPrefab , Room.GetStartPosition().position , Quaternion.identity , null);
        characterInstance.GetComponent<PlayerNetworkManager>().nrp = nrp;

        NetworkServer.Spawn(characterInstance, sender);

        NetworkServer.ReplacePlayerForConnection(sender, characterInstance, true);
    }
}

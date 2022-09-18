using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Examples.NetworkRoom;
using System.Linq;

public class CharacterLobbyDisplay : NetworkBehaviour
{
    [SerializeField] List<GameObject> characterPreviews = new List<GameObject>();

    private NetworkRoomManagerExt room;
    private NetworkRoomManagerExt Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkRoomManagerExt;
        }
    }

    // Update is called once per frame
    void Update()
    {
        var allPlayers = Room.roomSlots;

        if (allPlayers.Count == characterPreviews.Count) return;

        UpdatePlayersDisplay();
    }

    void UpdatePlayersDisplay()
    {
        foreach (var item in characterPreviews)
        {
            Destroy(item);
        }
        characterPreviews = new List<GameObject>();

        var allPlayers = Room.roomSlots;

        for (int i = 0; i < allPlayers.Count; i++)
        {
            GameObject p = Instantiate(Room.characters[allPlayers[i].GetComponent<NetworkRoomPlayer>().Character].CharacterPreviewPrefab);
            p.transform.position = new Vector3(allPlayers.Count - (i+1), 0, -i);
            characterPreviews.Add(p);
        }
    }
}

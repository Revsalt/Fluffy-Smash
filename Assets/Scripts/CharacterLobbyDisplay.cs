using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Examples.NetworkRoom;
using System.Linq;
using System;
using TMPro;

public class CharacterLobbyDisplay : NetworkBehaviour
{
    [SerializeField] List<RoomPlayerInfo> characterPreviews = new List<RoomPlayerInfo>();

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
            Destroy(item.gameObject);
        }
        characterPreviews = new List<RoomPlayerInfo>();

        var allPlayers = Room.roomSlots;

        for (int i = 0; i < allPlayers.Count; i++)
        {
            GameObject p = new GameObject(allPlayers[i].netId.ToString());

            p.transform.position = new Vector3(allPlayers.Count - (i+1), 0, -i);

            RoomPlayerInfo rpi = p.AddComponent<RoomPlayerInfo>();
            rpi.nrpe = Room.roomSlots[i].GetComponent<NetworkRoomPlayerExt>();

            characterPreviews.Add(rpi);
        }
    }
}

[Serializable]
class RoomPlayerInfo : MonoBehaviour
{   
    [HideInInspector] public TextMeshProUGUI usernametxt;

    private NetworkRoomManagerExt room;
    private NetworkRoomManagerExt Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkRoomManagerExt;
        }
    }

    public NetworkRoomPlayerExt nrpe;

    int oldindex = -1;
    GameObject model = null;

    void Start()
    {
        GameObject g = (GameObject)Instantiate(Resources.Load("UserNameDispaly"), gameObject.transform);
        usernametxt = g.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (oldindex != nrpe.Character || !model)
        {
            if (model) Destroy(model);
            model = Instantiate(Room.characters[nrpe.Character].CharacterPreviewPrefab , transform.position , Quaternion.identity , transform);
            oldindex = nrpe.Character;
        }

        string rd = "";
        if (nrpe.readyToBegin) {rd = "Ready";} else { rd = "NotReady";} 

        usernametxt.text = nrpe.username + " " + rd;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Examples.NetworkRoom;

public class CharacterSelect : NetworkBehaviour
{
    public static CharacterSelect instance;

    [SerializeField] private GameObject characterSelectDisplay = default;

    private int currentCharacterIndex = 0;

    private NetworkRoomManagerExt room;
    private NetworkRoomManagerExt Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkRoomManagerExt;
        }
    }

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        characterSelectDisplay.SetActive(false);
    }

    void Select()
    {
        NetworkRoomPlayer roomplayer = null;

        foreach (var item in Room.roomSlots)
        {
            if (item.netIdentity.isLocalPlayer)
            {
                roomplayer = item;
            }
        }

        roomplayer.CmdSendCharacterIndex(roomplayer.gameObject , currentCharacterIndex);
    }

    public void Right()
    {
        //characterInstances[currentCharacterIndex].SetActive(false);

        currentCharacterIndex = (currentCharacterIndex + 1) % Room.characters.Length;

        //characterInstances[currentCharacterIndex].SetActive(true);
        //characterNameText.text = Room.characters[currentCharacterIndex].CharacterName;

        Select();
    }

    public void Left()
    {
        //characterInstances[currentCharacterIndex].SetActive(false);

        currentCharacterIndex--;
        if (currentCharacterIndex < 0)
        {
            currentCharacterIndex += Room.characters.Length;
        }

        //characterInstances[currentCharacterIndex].SetActive(true);
        //characterNameText.text = Room.characters[currentCharacterIndex].CharacterName;

        Select();
    }
}
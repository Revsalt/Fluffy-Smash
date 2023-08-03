using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Cinemachine;

public class CharacterSelect : NetworkBehaviour
{
    public static CharacterSelect instance;

    [SerializeField] private CinemachineVirtualCamera characterSelectDisplay = default;

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

    public override void OnStartClient()
    {
        base.OnStartClient();
        characterSelectDisplay.m_Priority = 2;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        characterSelectDisplay.m_Priority = 0;
    }

    public void Select(int index)
    {
        currentCharacterIndex = index;

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
}
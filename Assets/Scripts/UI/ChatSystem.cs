using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class ChatSystem : NetworkBehaviour
{
    [SerializeField]
    private int maximumMessagesOnView = 2;

    [SerializeField]
    private GameObject chatUI = null;

    [SerializeField]
    private Text chatPrefab = null;

    [SerializeField]
    private InputField inputField = null;


    private List<string> ChatMsgs = new List<string>();


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            if (inputField.gameObject.activeSelf)
            {
                //Close input field
                inputField.gameObject.SetActive(false);

                //Send msg
                SendMessageClient(inputField.text);

                //Clear msg
                inputField.text = "";
            }
            else
            {
                //Open input field
                inputField.gameObject.SetActive(true);
                if (!inputField.isFocused)
                {
                    inputField.ActivateInputField();
                    inputField.Select();
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(inputField.gameObject.activeSelf)
            {
                inputField.gameObject.SetActive(false);
            }
        }
    }

    public void OnSubmit(string msg)
    {

    }

    void SendMessageClient(string msg)
    {
        if(msg == "")
        {
            return;
        }

        List<PlayerNetworkManager> AllPlayers = FindObjectsOfType<PlayerNetworkManager>().ToList();

        string username = "unkown";

        foreach (PlayerNetworkManager player in AllPlayers)
        {
            if (player.isLocalPlayer)
            {
                username = player.nrp.username;
                break;
            }
        }

        CmdSendMessage(username + ": " + msg);
    }
        

    [Command(requiresAuthority = false)]
    void CmdSendMessage(string msg)
    {
        RpcSendMessage(msg);
    }


    [ClientRpc]
    void RpcSendMessage(string msg)
    {
        //Destroy all children of chatUI
        foreach(Transform child in chatUI.transform)
        {
            Destroy(child.gameObject);
        }
        
        //Add message to our list
        if(ChatMsgs.Count >= maximumMessagesOnView)
        {
            //This means we need to delete the oldest element 
            ChatMsgs.RemoveAt(0);
        }
        ChatMsgs.Add(msg);

        //Add all messages to the chatUI
        foreach(string s in ChatMsgs)
        {
            Text text = Instantiate(chatPrefab, chatUI.transform);

            text.text = s;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class PlayerNetworkManager : NetworkBehaviour
{
    [SyncVar]public NetworkRoomPlayer nrp;

    [HideInInspector] public TextMeshProUGUI usernametxt;
    private void Start()
    {
        if (isLocalPlayer)
        {
            foreach (var item in GetComponent<PlayerController>().Cameras)
            {
                item.SetActive(true);
            }
        }
        else
        {
            GameObject g = (GameObject)Instantiate(Resources.Load("UserNameDispaly"), gameObject.transform);
            usernametxt = g.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        if (!isLocalPlayer) usernametxt.text = nrp.username;
        gameObject.name = "Player : " + nrp.username;


        if (transform.position.y < -2)
            GetComponent<PlayerController>().SetPlayerPosition(NetworkRoomManager.singleton.GetStartPosition().position);
    }
}

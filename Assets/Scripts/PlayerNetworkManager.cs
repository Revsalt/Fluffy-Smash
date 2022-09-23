using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using Cinemachine;

public class PlayerNetworkManager : NetworkBehaviour
{
    [SyncVar]public NetworkRoomPlayer nrp;

    [HideInInspector] public TextMeshProUGUI usernametxt;
    private void Start()
    {
        if (isLocalPlayer)
        {
            GetComponent<PlayerController>().DisableMovment(true);

            foreach (var item in GetComponent<PlayerController>().Cameras)
            {
                item.SetActive(true);
            }

            //GameObject g = (GameObject)Instantiate(Resources.Load("IndicatorController"), transform.position, transform.rotation);
            //g.GetComponent<UIController>().cam = GetComponent<DefaultCharacterEffects>().m_Camera;
        }
        else
        {
            GameObject g = (GameObject)Instantiate(Resources.Load("UserNameDispaly"), gameObject.transform);
            usernametxt = g.GetComponentInChildren<TextMeshProUGUI>();
            //gameObject.AddComponent<TargetObject>();
        }
    }

    void Update()
    {
        if (!isLocalPlayer) usernametxt.text = nrp.username;
        gameObject.name = "Player : " + nrp.username;


        if (transform.position.y < -2)
            GetComponent<TagLogic>().QuickRespawn();
    }
}

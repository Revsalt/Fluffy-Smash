using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerNetworkManager : NetworkBehaviour
{
    [SyncVar]public NetworkRoomPlayer nrp;


    private void Start()
    {
        Instantiate(Resources.Load("UserNameDispaly"), gameObject.transform);

        if (isLocalPlayer)
        {
            foreach (var item in GetComponent<PlayerController>().Cameras)
            {
                item.SetActive(true);
            }
        }
    }

    void Update()
    {
        GetComponentInChildren<BillBoard>().GetComponentInChildren<Text>().text = nrp.username;
        gameObject.name = "Player : " + nrp.username;

        if (transform.position.y < -40)
            GetComponent<PlayerController>().SetPlayerPosition(NetworkRoomManager.singleton.GetStartPosition().position);
    }
}

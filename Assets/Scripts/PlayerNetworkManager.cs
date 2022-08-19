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
    }

    void Update()
    {
        GetComponentInChildren<BillBoard>().GetComponentInChildren<Text>().text = nrp.username;
        gameObject.name = "Player : " + nrp.username;
    }
}

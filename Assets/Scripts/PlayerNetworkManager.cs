using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerNetworkManager : NetworkBehaviour
{
    public NetworkRoomPlayer nrp;
    [SyncVar]public string userName;
    public Text userNameText;

    
    void Update()
    {
        if (isServer)
            userName = nrp.username;

        userNameText.text = userName;
    }
}

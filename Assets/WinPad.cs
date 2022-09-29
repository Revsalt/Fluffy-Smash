using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WinPad : NetworkBehaviour
{
    public string TeamName = "None";

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && other.GetComponent<TagLogic>().TeamName != TeamName && other.GetComponentInChildren<TouchDownBall>())
        {   
            other.GetComponentInChildren<TouchDownBall>().RpcDropBall(other.GetComponentInChildren<TouchDownBall>().gameObject , Vector3.zero);
            NetworkServer.Destroy(other.GetComponentInChildren<TouchDownBall>().gameObject);
            RoundSystemTouchDown.instace.RoundEnded(other.GetComponent<TagLogic>().TeamName.ToUpper() + " WINS" , other.GetComponent<TagLogic>().TeamName);
            
            other.GetComponent<TagLogic>().isTagger = true;
        }
    }
}

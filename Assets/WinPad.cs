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
        if (other.tag == "Player" && other.GetComponent<Health>().TeamName != TeamName && other.GetComponentInChildren<TouchDownBall>())
        {   
            other.GetComponentInChildren<TouchDownBall>().RpcDropBall(other.GetComponentInChildren<TouchDownBall>().gameObject , Vector3.zero);
            NetworkServer.Destroy(other.GetComponentInChildren<TouchDownBall>().gameObject);
            RoundSystemTouchDown.instance.RoundsEnded(other.GetComponent<Health>().TeamName.ToUpper() + " WINS");
            
            other.GetComponent<Health>().canInfluenceDamage = true;
        }
    }
}

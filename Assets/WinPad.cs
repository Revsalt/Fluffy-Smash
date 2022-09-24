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
            NetworkServer.Destroy(other.GetComponentInChildren<TouchDownBall>().gameObject);
            RoundSystemTouchDown.instace.RoundEnded(other.GetComponent<TagLogic>().TeamName.ToUpper() + " WINS");

            other.GetComponent<TagLogic>().isTagger = true;
        }
    }
}

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
        if (other.tag == "Player" && other.GetComponentInChildren<TouchDownBall>())
        {   
            other.GetComponentInChildren<TouchDownBall>().ResetBall();
            //RoundSystemTouchDown.instance.RoundsEnded(other.GetComponent<Health>().TeamName.ToUpper() + " WINS");
            
            other.GetComponent<Health>().canInfluenceDamage = true;
        }
    }
}

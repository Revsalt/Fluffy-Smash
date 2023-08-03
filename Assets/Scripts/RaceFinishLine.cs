using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RaceFinishLine : MonoBehaviour
{
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerNetworkManager>())
            FindObjectOfType<RoundSystemRace>()?.HasPassed(other.GetComponent<PlayerNetworkManager>());
    }
}

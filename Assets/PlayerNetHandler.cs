using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerNetHandler : NetworkBehaviour
{
    public GameObject cam;

    private void Start()
    {
        cam.SetActive(isLocalPlayer);
        GetComponent<PlayerController>().enabled = isLocalPlayer;
    }
}

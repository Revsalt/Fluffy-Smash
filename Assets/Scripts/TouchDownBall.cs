using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class TouchDownBall : NetworkBehaviour
{
    public static TouchDownBall instance;

    private void Start()
    {
        instance = this;
    }

    [SerializeField] KeyCode throwKeyBind;

    private void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(throwKeyBind) && transform.parent.GetComponent<NetworkIdentity>().isLocalPlayer)
            {
                
            }
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerNetworkManager>())
        {
            CarryBall(other.GetComponent<NetworkIdentity>());
        }
    }


    [Command]
    void LaunchBall(Vector3 dir)
    {
        transform.parent = null;

    }

    [ClientRpc]
    private void CarryBall(NetworkIdentity ntd)
    {
        TouchDownBall.instance.transform.SetParent(ntd.transform);
        TouchDownBall.instance.transform.localPosition = new Vector3(0, 2, 0);
    }
}

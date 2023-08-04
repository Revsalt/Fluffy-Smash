using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ServerAuthoritativeTransform : NetworkBehaviour
{
    Vector3 old_serv_pos = Vector3.zero;

    [SyncVar] public ClientInput clientInput = new ClientInput();

    ClientInput old_ci = new ClientInput();

    [HideInInspector] public GameObject server_position;

    private void Start()
    {
        if (isClient)
        {
            server_position = Instantiate(Resources.Load("Player_Visualization") as GameObject, Vector3.zero, Quaternion.identity);
        }
        
        old_serv_pos = transform.position;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        clientInput.cameraRotation = GetComponent<PlayerController>().piviot_M.transform.localRotation;
        clientInput.movementAxis = new Vector2(Input.GetAxisRaw("Horizontal") , Input.GetAxisRaw("Vertical"));

        if (!old_ci.Equals(clientInput.movementAxis))
        {
            SendInput(clientInput);

            old_ci.movementAxis = clientInput.movementAxis;
            old_ci.cameraRotation = clientInput.cameraRotation;
        }
    }
       
    [Command]
    void SendInput(ClientInput ci , NetworkConnectionToClient sender = null)
    {
        sender.identity.gameObject.GetComponent<ServerAuthoritativeTransform>().clientInput = ci;
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        GetComponent<PlayerController>().piviot_M.transform.localRotation = clientInput.cameraRotation;

        if (old_serv_pos != transform.position)
        {
            SendNewPosition(transform.position , GetComponent<NetworkIdentity>());
            old_serv_pos = transform.position;
        }
    }

    [ClientRpc]
    public void SendNewPosition(Vector3 new_pos , NetworkIdentity identity)
    {
        identity.GetComponent<ServerAuthoritativeTransform>().server_position.transform.position = new_pos;
        identity.transform.position = new_pos;
    }
}

[Serializable]
public class ClientInput
{
    public Vector2 movementAxis = new Vector2() { x = 0 , y = 0 };
    public Quaternion cameraRotation = Quaternion.identity;
    public bool jump , l_mouse , r_mouse , l_shift;
}

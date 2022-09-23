using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TouchDownBall : NetworkBehaviour
{
    [ClientCallback]
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && other.GetComponent<TagLogic>())
        {
             if (!other.GetComponent<NetworkIdentity>().isLocalPlayer) return;

            transform.SetParent(other.transform);
            transform.localPosition = new Vector3(0,1,0);
            CmdPickUpBall(transform , other.transform);
        }
    }

    [Command(requiresAuthority = false)]
    void CmdPickUpBall(Transform ball , Transform player)
    {
        ball.SetParent(player.transform);
        ball.transform.localPosition = new Vector3(0,1,0);
        RpcPickUpBall(ball , player);
    }

    [ClientRpc]
    void RpcPickUpBall(Transform ball , Transform player)
    {
        if (player.GetComponent<NetworkIdentity>().isLocalPlayer) return;
        
        ball.SetParent(player.transform);
        ball.transform.localPosition = new Vector3(0,1,0);
    }

    [ClientRpc]
    public void RpcDropBall(Transform ball , Vector3 pos)
    {
        ball.SetParent(null);
        ball.position = pos;
    }

    [ServerCallback]
    void FixedUpdate()
    {
        if (transform.parent != null && transform.parent.GetComponent<TagLogic>().IsDead)
        {
            transform.SetParent(null);
            RpcDropBall(transform , transform.position);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class TouchDownBall : NetworkBehaviour
{
    
    [ClientCallback]
    void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Player" && collision.GetComponent<TagLogic>())
        {
            if (!collision.GetComponent<NetworkIdentity>().isLocalPlayer) return;

            GetComponent<SphereCollider>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            transform.SetParent(collision.transform);
            transform.localPosition = new Vector3(0,1.5f,0);
            
            CmdPickUpBall(transform , collision.transform);
        }
    }

    [Command(requiresAuthority = false)]
    void CmdPickUpBall(Transform ball , Transform player)
    {
        player.GetComponent<TagLogic>().isTagger = false;

        RpcPickUpBall(ball , player);
    }

    [ClientRpc]
    void RpcPickUpBall(Transform ball , Transform player)
    {
        ball.GetComponent<SphereCollider>().enabled = false;
        ball.GetComponent<Rigidbody>().isKinematic = true;
        ball.SetParent(player.transform);
        ball.transform.localPosition = new Vector3(0,1.5f,0);
    }

    [ClientRpc]
    public void RpcDropBall(GameObject ball , Vector3 pos)
    {
        ball.GetComponent<SphereCollider>().enabled = true;
        ball.GetComponent<Rigidbody>().isKinematic = false;
        ball.transform.SetParent(null);
        ball.transform.position = pos;
    }

    [ServerCallback]
    void FixedUpdate()
    {
        if (transform.parent != null && transform.parent.GetComponent<TagLogic>().IsDead)
        {
            GetComponent<SphereCollider>().enabled = true;
            GetComponent<Rigidbody>().isKinematic = false;
            transform.parent.GetComponent<TagLogic>().isTagger = true;
            transform.SetParent(null);
            RpcDropBall(gameObject , transform.position);
        }
    }

}

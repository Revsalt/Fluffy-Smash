using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class TouchDownBall : NetworkBehaviour
{
    Rigidbody rb;

    void Start()
    {
        if (isServer) rb = gameObject.AddComponent<Rigidbody>();
    }

    [ServerCallback]
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Player" && !transform.parent)
        {
            GetComponent<SphereCollider>().enabled = false;
            rb.isKinematic = true;
            transform.SetParent(collision.collider.transform);
            transform.localPosition = new Vector3(0, 1.5f, 0);

            RpcPickUpBall(transform, collision.collider.transform);

            collision.collider.transform.GetComponent<Health>().canInfluenceDamage = true;
        }
    }

    [ServerCallback]
    public void Drop()
    {
        transform.SetParent(null);
        GetComponent<SphereCollider>().enabled = true;
        rb.isKinematic = false;

        RpcDropBall(gameObject);
    }

    [ClientRpc]
    void RpcPickUpBall(Transform ball, Transform player)
    {
        ball.SetParent(player.transform);
        ball.transform.localPosition = new Vector3(0, 1.5f, 0);
    }

    [ClientRpc]
    public void RpcDropBall(GameObject ball)
    {
        ball.transform.SetParent(null);
    }

    [ServerCallback]
    public void ResetBall()
    {
        GetComponent<SphereCollider>().enabled = true;
        rb.isKinematic = false;
        transform.SetParent(null);

        transform.position = FindObjectsOfType<NetworkStartPosition>()
        [UnityEngine.Random.Range(0, FindObjectsOfType<NetworkStartPosition>().Length)].transform.position;
    }

    /*
    [ServerCallback]
    void FixedUpdate()
    {
        if (transform.parent != null && transform.parent.GetComponent<Health>().IsDead)
        {
            //GetComponent<SphereCollider>().enabled = true;
            //GetComponent<Rigidbody>().isKinematic = false;
            transform.parent.GetComponent<Health>().canInfluenceDamage = true;
            transform.SetParent(null);
            RpcDropBall(gameObject , transform.position);
        }

        if (transform.position.y < -60)
            transform.position = Vector3.zero;
    }
    */

}

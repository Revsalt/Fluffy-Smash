using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOverTimeServer : NetworkBehaviour
{
    public Vector3 Axis;
    public float speed;

    void Start()
    {
        enabled = (isServer);   
    }

    void Update()
    {
        transform.Rotate(Axis * speed * Time.deltaTime);
    }
}

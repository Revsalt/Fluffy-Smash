using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Cymbal : NetworkBehaviour
{
    public float speed = 3;
    Vector2 mouseStart;

    private void Start()
    {
        mouseStart = Input.mousePosition;
    }

    void Update()
    {
        Vector3 move = transform.position + (transform.forward * speed * Time.deltaTime);
        move += transform.TransformDirection(new Vector3(Input.mousePosition.x - mouseStart.x , Input.mousePosition.y - mouseStart.y , 0)) * (speed / 4) * Time.deltaTime;

        transform.position = move;
    }
}

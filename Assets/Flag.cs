using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, 4))
        {
            transform.forward =
                Vector3.Slerp(transform.forward, GetComponent<Rigidbody>().velocity.normalized, Time.deltaTime * 15);
        }
    }
}

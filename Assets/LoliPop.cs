using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoliPop : MonoBehaviour
{
    void Update()
    {
        if (transform.position.y < -20)
            GetComponent<Rigidbody>().isKinematic = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollDeathExplosion : MonoBehaviour
{
    void Start()
    {
        foreach (var item in GetComponentsInChildren<Rigidbody>())
        {
            item.AddExplosionForce(9000 * Time.deltaTime , transform.position , 10);
        }
    }
}

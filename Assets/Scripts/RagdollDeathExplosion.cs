using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollDeathExplosion : MonoBehaviour
{
    void Start()
    {
        foreach (var item in GetComponentsInChildren<Rigidbody>())
        {
            item.AddExplosionForce(200 * Time.deltaTime , transform.position , 5);
        }
    }
}

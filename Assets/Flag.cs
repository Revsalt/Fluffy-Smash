using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    public string Team;
    [SerializeField] SkinnedMeshRenderer flagMesh;

    private void Start()
    {
        flagMesh.material.color = Team == "Red" ? Color.red : Color.blue;
    }

    void FixedUpdate()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, 4))
        {
            transform.forward =
                Vector3.Slerp(transform.forward, GetComponent<Rigidbody>().velocity.normalized, Time.deltaTime * 15);
        }
    }
}

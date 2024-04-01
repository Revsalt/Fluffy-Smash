using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureZone : NetworkBehaviour
{
    public string Team;
    [SerializeField] float radius;

    [ServerCallback]
    public Collider[] GetPlayersInside()
    {
        return Physics.OverlapSphere(transform.position, radius);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

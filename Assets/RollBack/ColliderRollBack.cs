using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Threading.Tasks;

public class ColliderRollBack : NetworkBehaviour
{
    private const float tickTimerMax = 0.2f;
    float tickTimer = 0;
    public Vector3[] ServerPostions;

    [ServerCallback]
    public void Start()
    {
        Debug.Log(NetworkTime.time);
    }



}

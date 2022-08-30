using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : NetworkBehaviour
{
    public float duration;

    // Start is called before the first frame update
    void Start()
    {
        if (!isServer)
            return;

        StartCoroutine(time());

        IEnumerator time()
        {
            yield return new WaitForSeconds(duration);
            NetworkServer.Destroy(gameObject);
        }
    }
}

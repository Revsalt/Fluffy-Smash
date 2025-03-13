using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Flag_CP : NetworkBehaviour
{
    private void Start()
    {
        StartCoroutine(Throw());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isServer && other.GetComponent<NetworkIdentity>())
        {
            transform.parent = other.transform;
            transform.localPosition = Vector3.zero + new Vector3(0, 1, 0);

            HePickedUP(other.GetComponent<NetworkIdentity>());

            //for client prediction just incase ->
            // did he infact pick it up ? 
            // tell everyone he picked it up !
        }
    }

    [ClientRpc]
    public void HePickedUP(NetworkIdentity thePicker)
    {
        transform.parent = thePicker.transform;
        transform.localPosition = Vector3.zero + new Vector3(0, 1, 0);
    }

    public IEnumerator Throw()
    {
        Debug.Log("thrw");

        CharacterController cc = GetComponent<CharacterController>();

        cc.SimpleMove(transform.forward * 100);

        yield return new WaitForSeconds(1);

        StartCoroutine(Throw());
    }
}

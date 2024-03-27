using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerNetworkManager : NetworkBehaviour
{
    [SerializeField] Transform flagHook;
    [SerializeField] GameObject flagPrefab;
    [SyncVar] public string Role = "";

    public void Update()
    {
        if (isServer)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 2);

        }

        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            SendDropFlag(GetComponent<PlayerAnimations>().virtualCamera.transform.forward);
        }
    }

    [ClientRpc]
    public void GiveFlag(NetworkIdentity target)
    {
        target.GetComponent<PlayerNetworkManager>().flagHook.gameObject.SetActive(true);
    }

    [Command(requiresAuthority = false)]
    public void SendDropFlag(Vector3 direction , NetworkConnectionToClient sender = null)
    {
        GameObject flag = Instantiate(flagPrefab, transform.position + direction * 12, Quaternion.LookRotation(direction, Vector3.up) , null);
        NetworkServer.Spawn(flag);

        flag.GetComponent<Rigidbody>().AddForce(direction * 90000 * Time.deltaTime, ForceMode.Impulse);
        flag.transform.forward = direction;

        RpcDropFlag(sender.identity);
    }

    [ClientRpc]
    public void RpcDropFlag(NetworkIdentity target)
    {
        target.GetComponent<PlayerNetworkManager>().flagHook.gameObject.SetActive(false);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Flag")
        {
            if (flagHook.gameObject.activeInHierarchy == true) return;

            flagHook.gameObject.SetActive(true);
            NetworkServer.Destroy(other.gameObject);

            GiveFlag(GetComponent<NetworkIdentity>());
        }
    }

}

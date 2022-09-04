using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class TagLogic : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnChangeTaggerState))] public bool isTagger = false;
    bool CanAttack = false;
    public float TagRadius = 2;

    [Header("InTag")]
    public UnityEvent StartTag;
    public UnityEvent EndTag;
    [Header("InDeath")]
    public UnityEvent OnDeath;

    PlayerController myPlayer;

    public void SetCanAttack(bool b)
    {
        if (isLocalPlayer) CanAttack = b;

    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (CanAttack)
        {
            foreach (var player in FindObjectsOfType<TagLogic>())
            {
                if (!player.GetComponent<NetworkIdentity>().isLocalPlayer && !player.GetComponent<TagLogic>().isTagger && Vector3.Distance(player.transform.position , transform.position) < TagRadius) // if anyone is close to me
                {
                    Kill(player.GetComponent<NetworkIdentity>());
                    CanAttack = false;
                    break;
                }  
            }
        }
    }

    [Command]
    public void Kill(NetworkIdentity player)
    {
        player.GetComponent<TagLogic>().isTagger = true;
        RpcKill(player.connectionToClient);


    }

    [TargetRpc]
    void RpcKill(NetworkConnection nc)
    {
        nc.identity.GetComponent<TagLogic>().OnDeath.Invoke();
        nc.identity.GetComponent<PlayerController>().DisableInput(true);
    }

    public void OnChangeTaggerState(bool oldb, bool newb)
    {
        if (newb)
            GetComponent<PlayerNetworkManager>().usernametxt.color = Color.red;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(Vector3.zero, TagRadius);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TagLogic : NetworkBehaviour
{
    [SyncVar] public bool isTagger = false;
    [SyncVar]public bool isTagged = false;
    public bool CanTag = false;
    public Transform TagPosition;
    public float TagRadius = 2;

    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (CanTag)
        {
            Collider[] players = Physics.OverlapSphere(TagPosition.position , TagRadius);
            foreach (var item in players)
            {
                if (item.GetComponent<TagLogic>() && !item.GetComponent<NetworkIdentity>().isLocalPlayer)
                {
                    CmdIsTagged(item.gameObject);
                    CanTag = false;
                }
                return;
            }
        }
    }

    [Command]
    public void CmdIsTagged(GameObject item)
    {
        item.GetComponent<TagLogic>().isTagged = true;
        item.GetComponent<TagLogic>().isTagger = true;
        MakeTagger(item.GetComponent<NetworkIdentity>().connectionToClient);
    }

    [TargetRpc]
    public void MakeTagger(NetworkConnection ntd)
    {
        StartCoroutine(Tag());

        IEnumerator Tag()
        {
            ntd.identity.GetComponent<Player>().enabled = false;
            yield return new WaitForSeconds(2);
            ntd.identity.GetComponent<Player>().enabled = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawSphere(TagPosition.position, TagRadius);
    }
}

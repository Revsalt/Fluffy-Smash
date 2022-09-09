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
    public UnityEvent OnRespawn;
    [Header("DeathEffect")]
    [SerializeField] Transform deathInstantiationParent;
    [SerializeField] GameObject deathModel;

    PlayerController myPlayer;

    private void Start()
    {
        myPlayer = GetComponent<PlayerController>();
    }

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
                    Kill(player.GetComponent<NetworkIdentity>() , GetComponent<NetworkIdentity>());
                    CanAttack = false;
                    Instantiate(Resources.Load("DeathIcon"));
                    break;
                }  
            }
        }
    }

    [Command]
    public void Kill(NetworkIdentity player , NetworkIdentity killedBY)
    {
        player.GetComponent<TagLogic>().isTagger = true;
        RpcKill(player , killedBY);
    }

    [ClientRpc]
    void RpcKill(NetworkIdentity nc , NetworkIdentity killedBy)
    {
        nc.GetComponent<TagLogic>().OnDeath.Invoke();
        nc.GetComponent<CharacterController>().enabled = false;

        if (nc.isLocalPlayer)
        {

            nc.GetComponent<PlayerController>().DisableInput(true);
            nc.GetComponent<PlayerController>().folllowTarget = killedBy.transform;
            nc.GetComponent<TagLogic>().Resapawn(4);
        }

    }

    public void SetParentNull(GameObject g)
    {
        g.transform.SetParent(null);
    }
    public void DisableChildrensMeshRenders(GameObject g)
    {
        foreach (var item in g.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            item.enabled = false;
        }
    }

    public void EnableChildrensMeshRenders(GameObject g)
    {
        foreach (var item in g.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            item.enabled = true;
        }
    }

    public void OnChangeTaggerState(bool oldb, bool newb)
    {
        if (newb && GetComponent<PlayerNetworkManager>().usernametxt)
            GetComponent<PlayerNetworkManager>().usernametxt.color = Color.red;
    }

    public void Resapawn(int time)
    {
        StartCoroutine(delay());

        IEnumerator delay()
        {
            var l = FindObjectsOfType<NetworkStartPosition>();

            yield return new WaitForSeconds(time);

            myPlayer.SetPlayerPosition(l[Random.Range(0,l.Length)].transform.position);
            myPlayer.DisableInput(false);

            var r = (GameObject)Instantiate(deathModel , deathInstantiationParent.transform);
            r.transform.localPosition = Vector3.zero;

            myPlayer.folllowTarget = transform;

            OnRespawn.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(Vector3.zero, TagRadius);
    }
}

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

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Kill(GetComponent<NetworkIdentity>(), GetComponent<NetworkIdentity>());
            Instantiate(Resources.Load("DeathIcon"));
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
        nc.GetComponent<TagLogic>().Resapawn(4);

        if (nc.isLocalPlayer)
        {
            nc.GetComponent<PlayerController>().DisableInput(true);
            StartCoroutine(KillCamDelay());

            IEnumerator KillCamDelay()
            {
                yield return new WaitForSeconds(2);
                nc.GetComponent<PlayerController>().folllowTarget = killedBy.transform;
            }
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

            myPlayer.folllowTarget = transform;

            OnRespawn.Invoke();
        }
    }

    public void SpawnDeadModel(GameObject deadModel)
    {
        GameObject bodyPart = (GameObject)Instantiate(deadModel.gameObject, deadModel.transform.position , deadModel.transform.rotation , null);
        bodyPart.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(Vector3.zero, TagRadius);
    }
}

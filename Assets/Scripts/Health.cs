using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class Health : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnChangeDamageState))] public bool canInfluenceDamage = false;
    [SyncVar(hook = nameof(OnChangeTeam))] public string TeamName = "none";
    bool CanAttack = false;
    public float damageRadius = 2;

    [Header("InAttack")]
    public UnityEvent StartAttack;
    public UnityEvent EndAttack;
    
    [Header("InDeath")]
    public UnityEvent OnDeath;
    public UnityEvent OnRespawn;

    [SyncVar]public bool IsDead;

    PlayerController myPlayer;

    private void Start()
    {
        myPlayer = GetComponent<PlayerController>();
    }

    public void SetCanAttack(bool b)
    {
        if (isLocalPlayer) CanAttack = b;

    }

    public void OnChangeTeam(string olds , string news)
    {
        Color c = Color.black;
        
        if (news == "RedTeam") { c = Color.red; } 
        else if (news == "BlueTeam"){ c = Color.blue;}

        GetComponent<PlayerController>().playerModel.GetComponentInChildren<OutLineColor>().SetColor(c);
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (CanAttack)
        {
            foreach (var player in FindObjectsOfType<Health>())
            {
                if (!player.GetComponent<NetworkIdentity>().isLocalPlayer && (TeamName != player.TeamName || TeamName == "none" || player.TeamName == "none") && Vector3.Distance(player.transform.position , transform.position) < damageRadius) // if anyone is close to me
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
        player.GetComponent<Health>().canInfluenceDamage = true;

        RpcKill(player , killedBY);
    }

    [ClientRpc]
    void RpcKill(NetworkIdentity nc , NetworkIdentity killedBy)
    {
        nc.GetComponent<Health>().OnDeath.Invoke();
        nc.GetComponent<CharacterController>().enabled = false;
        nc.GetComponent<Health>().Resapawn(4);

        if (nc.isLocalPlayer)
        {
            nc.GetComponent<PlayerController>().DisableInput(true);
            StartCoroutine(KillCamDelay());

            IsDead = true;
            
            IEnumerator KillCamDelay()
            {
                yield return new WaitForSeconds(2);
                nc.GetComponent<PlayerController>().folllowTarget = killedBy.transform;
                IsDead = false;
            }
        }
    }

    public void OnChangeDamageState(bool oldb, bool newb)
    {
        if (newb && GetComponent<PlayerNetworkManager>().usernametxt)
            GetComponent<PlayerNetworkManager>().usernametxt.color = Color.red;
    }

    public void Resapawn(int time)
    {
        StartCoroutine(delay());

        IEnumerator delay()
        {
            yield return new WaitForSeconds(time);

            myPlayer.SetPlayerPosition(NetworkStartPosition.GetSpawnPoistionRandomAtTeam(TeamName));
            myPlayer.DisableInput(false);

            myPlayer.folllowTarget = transform;

            OnRespawn.Invoke();
        }
    }

    public void QuickRespawn()
    {
        myPlayer.SetPlayerPosition(NetworkStartPosition.GetSpawnPoistionRandomAtTeam(TeamName));
        myPlayer.DisableInput(false);

        myPlayer.folllowTarget = transform;

        OnRespawn.Invoke();
    }

    public void SpawnDeadModel(GameObject deadModel)
    {
        GameObject bodyPart = (GameObject)Instantiate(deadModel.gameObject, deadModel.transform.position , deadModel.transform.rotation , null);
        bodyPart.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(Vector3.zero, damageRadius);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mirror;

public class CaptureZone : NetworkBehaviour
{
    [SyncVar(hook = nameof(SetColor))]public Color ZoneColor;
    [SerializeField] Renderer[] renderers;
    [SerializeField] Transform animation_face;
    [SerializeField] List<PlayerNetworkManager> playersInside = new List<PlayerNetworkManager>();
    [SerializeField] float radius;
    [SerializeField] float current_charge_time;
    [SerializeField] float max_charge_time;
    public List<TeamZoneCounter> teamZoneCounters = new List<TeamZoneCounter>();

    Coroutine charge_coroutine = null;
    Coroutine zone_coroutine = null;

    [Header("Preview")]
    [SerializeField] bool viewModel = false;

    public void Activate()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }

    [ServerCallback]
    void Update()
    {
        if (!transform.GetChild(0).gameObject.activeInHierarchy) return;

        Collider[] c = Physics.OverlapSphere(transform.position, radius);

        playersInside = new List<PlayerNetworkManager>();
        foreach (var item in c)
        {
            if (item.GetComponent<PlayerNetworkManager>())
            {
                playersInside.Add(item.GetComponent<PlayerNetworkManager>());
            }
        }

        foreach (var item in renderers)
        {
            item.material.color = ZoneColor;
        }

        if (playersInside.Count == 0) return;

        foreach (var item in Team.AllTeams)
        {
            if (item.teamColor != ZoneColor && IsAllThisTeam(playersInside, item) && charge_coroutine == null)
            {
                charge_coroutine = StartCoroutine(StartCount(item));
            }
        }
    }

    IEnumerator StartCount(Team t)
    {
        for (float i = 0; (i <= max_charge_time) && (IsAllThisTeam(playersInside, t)); i += Time.deltaTime)
        {
            float s = i / max_charge_time;
            animation_face.localScale = new Vector3(s,s,s);

            if (animation_face.GetComponent<Renderer>().material.color != t.teamColor)
            {
                animation_face.GetComponent<Renderer>().material.color = t.teamColor;
                RpcSetMeshAnimationColor(GetComponent<NetworkIdentity>(), t.teamColor);
            }

            current_charge_time = i;
            yield return null;
        }

        current_charge_time = Mathf.RoundToInt(current_charge_time);

        if (current_charge_time == max_charge_time)
        {
            ZoneColor = t.teamColor;

            if (GetTeamZCFromTeam(t) == null)
            {
                teamZoneCounters.Add(new TeamZoneCounter() { team = t, duration = 0 });
            }

            if (zone_coroutine != null)
                StopCoroutine(zone_coroutine);

            zone_coroutine = StartCoroutine(StartCountingZoneTime(GetTeamZCFromTeam(t)));

            Debug.Log(current_charge_time + " = " + max_charge_time + " Charged");
        }
        else
        {
            Debug.Log(current_charge_time + " = " + max_charge_time + " No");
        }

        animation_face.localScale = Vector3.zero;
        StopCoroutine(charge_coroutine);
        charge_coroutine = null;

    }

    IEnumerator StartCountingZoneTime(TeamZoneCounter tzc)
    {
        while (true)
        {
            if (IsAllThisTeam(playersInside, tzc.team))
            {
                tzc.duration += Time.deltaTime;
            }
            else if (playersInside.Count == 0)
            {
                tzc.duration += Time.deltaTime;
            }

            yield return null;
        }
    }

    TeamZoneCounter GetTeamZCFromTeam(Team t)
    {
        foreach (var item in teamZoneCounters)
        {
            if (t == item.team) 
                return item;
        }

        return null;
    }

    [ClientRpc]
    void RpcSetMeshAnimationColor(NetworkIdentity ntd , Color c)
    {
        ntd.GetComponent<CaptureZone>().animation_face.GetComponent<Renderer>().material.color = c;
    }

    bool IsAllThisTeam(List<PlayerNetworkManager> lp, Team team)
    {
        if (lp.Count == 0)
            return false;

        foreach (var item in lp)
        {
            if (item.Team_m != team)
                return false;
        }

        return true;
    }

    public void SetColor(Color old_c , Color new_c)
    {
        foreach (var item in renderers)
        {
            item.material.color = new_c;
        }
    }

    private void OnDrawGizmos()
    {
        if (!EditorApplication.isPlaying)
            transform.GetChild(0).gameObject.SetActive(viewModel);
        else 
            return;

        if (!transform.GetChild(0).gameObject.activeInHierarchy) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

[System.Serializable]
public class TeamZoneCounter
{
    public Team team;
    public float duration;
}

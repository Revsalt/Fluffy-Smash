using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class NetworkStartPositionTeams : NetworkStartPosition
{
    public Team team = Team.None;
    public static Vector3 GetSpawnPoistionRandomAtTeam(Team team_)
    {
        List<NetworkStartPositionTeams> avaliablePositions = new List<NetworkStartPositionTeams>();

        foreach (var item in FindObjectsOfType<NetworkStartPositionTeams>())
        {
            if (item.team == team_)
            {
                avaliablePositions.Add(item);
            }
        }

        if (avaliablePositions.Count == 0)
            avaliablePositions = FindObjectsOfType<NetworkStartPositionTeams>().ToList();

        return avaliablePositions[Random.Range(0, avaliablePositions.Count)].transform.position;
    }
}

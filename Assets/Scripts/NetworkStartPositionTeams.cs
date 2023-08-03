using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class NetworkStartPositionTeams : NetworkStartPosition
{
    public TeamEnum team = TeamEnum.None;
    public static Transform GetSpawnPoistionRandomAtTeam(Team team_)
    {
        List<NetworkStartPositionTeams> avaliablePositions = new List<NetworkStartPositionTeams>();

        foreach (var item in FindObjectsOfType<NetworkStartPositionTeams>())
        {
            Team t = null;

            switch (item.team)
            {
                case TeamEnum.Red: t = Team.Red; break;
                case TeamEnum.Blue: t = Team.Blue; break;
                case TeamEnum.Yellow: t = Team.Yellow; break;
                case TeamEnum.Green: t = Team.Green; break;
                case TeamEnum.None: t = Team.None; break;
            }

            if (t == team_)
            {
                avaliablePositions.Add(item);
                Debug.Log("found " + t.teamName);
            }
        }

        if (avaliablePositions.Count == 0)
        {
            avaliablePositions = FindObjectsOfType<NetworkStartPositionTeams>().ToList();
            Debug.Log("No avilable positions");
        }

        return avaliablePositions[Random.Range(0, avaliablePositions.Count)].transform;
    }

    public enum TeamEnum
    {
        Red,
        Blue,
        Green,
        Yellow,
        None
    }
}

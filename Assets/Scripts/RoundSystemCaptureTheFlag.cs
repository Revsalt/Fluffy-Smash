using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class RoundSystemCaptureTheFlag : RoundSystem
{
    private void Start()
    {
        foreach (var item in FindObjectsOfType<CaptureZone>())
        {
            item.Activate();
        }
    }

    public string GetWinner()
    {
        CaptureZone[] all_captureZones = FindObjectsOfType<CaptureZone>();

        List<TeamZoneCounter> teams = new List<TeamZoneCounter>();

        foreach (var item in Team.AllTeams)
        {
            float team_total_duration = 0;

            foreach (var k in all_captureZones)
            {
                foreach (var j in k.teamZoneCounters)
                {
                    if (j.team == item)
                    {
                        team_total_duration += j.duration;
                    }
                }
            }

            teams.Add(new TeamZoneCounter() { team = item , duration = team_total_duration });
        }

        TeamZoneCounter winning_team = new TeamZoneCounter() { team = Team.None , duration = 0 };

        foreach (var item in teams)
        {
            if (item.duration > winning_team.duration)
                winning_team = item;
        }

        return winning_team.team.teamName;
    }

    [ServerCallback]
    public override void RoundsEnded()
    {
        RpcEndRound(GetWinner());

        StartCoroutine(WinDelay());

        IEnumerator WinDelay()
        {
            yield return new WaitForSeconds(2);
            Room.ServerChangeScene(Room.RoomScene);
        }

    }
}

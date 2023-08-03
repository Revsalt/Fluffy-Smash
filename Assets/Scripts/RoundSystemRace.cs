using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class RoundSystemRace : RoundSystem
{
    public List<PlayerNetworkManager> playersNotPassed = new List<PlayerNetworkManager>();

    public List<PlayerNetworkManager> winList = new List<PlayerNetworkManager>();

    [ServerCallback]
    public override void OnRoundStart()
    {
        base.OnRoundStart();

        playersNotPassed = GetAllPlayers();
    }

    [ServerCallback]
    public void HasPassed(PlayerNetworkManager pnm)
    {
        if (winList.Contains(pnm)) return;

        winList.Add(pnm);
        playersNotPassed.Remove(pnm);

        if (playersNotPassed.Count == 0)
        {
            RoundsEnded(winList[0].name + " WON!");
        }
    }
}

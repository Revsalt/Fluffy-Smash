using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class RoundSystemTag : RoundSystem
{

    public override void OnPlayerKill(NetworkIdentity theKiller)
    {
        base.OnPlayerKill(theKiller);

        theKiller.GetComponent<PlayerNetworkManager>().playerScores[0] += 1;

        foreach (var item in GetAllPlayers())
        {
            if (item.GetComponent<PlayerNetworkManager>()?.playerScores[0] >= 3)
            {
                RoundsEnded("Player * won");
            }
        }
    }
}

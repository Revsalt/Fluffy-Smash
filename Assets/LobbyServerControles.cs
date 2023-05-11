using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class LobbyServerControles : NetworkBehaviour
{
    public RoundSystemData rsd = new RoundSystemData();
    public static LobbyServerControles Instance;

    void Start()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void HandleInputDataMap(int val)
    {
        rsd.matchMap = val;
    }

    public void HandleInputDataGameMode(int val)
    {
        rsd.matchGameMode = val;
    }

    public void HandleInputDataRoundTime(int val)
    {

        rsd.roundTimeVal = val;
    }

}

public class RoundSystemData
{
    public float roundTimeVal;
    public int matchMap;
    public int matchGameMode;

    public float GetRoundTime()
    {
        float time_round = 2;

        switch (roundTimeVal)
        {
            case 0: time_round = 1; break;
            case 1: time_round = 2; break;
            case 2: time_round = 5; break;
            case 3: time_round = 10; break;
            case 4: time_round = 20; break;
            default: time_round = 2; break;
        }

        return time_round;
    }
}

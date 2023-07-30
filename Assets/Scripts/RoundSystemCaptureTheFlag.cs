using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class RoundSystemCaptureTheFlag : RoundSystem
{
    public override void OnRoundStart()
    {
        base.OnRoundStart();
        Instantiate(Resources.Load("Ball") , Vector3.zero + Vector3.up , Quaternion.identity , null);
    }
}

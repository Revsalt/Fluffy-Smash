using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ServerCamera : MonoBehaviour
{
    public CinemachineTargetGroup cmtg;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var item in FindObjectsOfType<PlayerController>())
        {
            if (!cmtg.m_Targets.ToList().Contains(new CinemachineTargetGroup.Target() { target = item.transform, radius = 1, weight = 1 }) )
            {
                cmtg.AddMember(item.transform , 1, 1);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMotor : MonoBehaviour
{
    void FixedUpdate()
    {
        transform.Translate(0 ,0 , Mathf.Sin(Time.time) / 10);
    }
}

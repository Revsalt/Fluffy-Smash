using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bamboo : MonoBehaviour
{
    public void LookAtObject(GameObject gameobject)
    {
        transform.LookAt(gameobject.transform.position);
    }

    public void StartBounce()
    {
        GetComponent<Animator>().SetTrigger("isBounce");
    }

    public void StopBounce()
    {
        GetComponent<Animator>().SetTrigger("Stop");
    }
}

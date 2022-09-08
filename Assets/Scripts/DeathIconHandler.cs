using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathIconHandler : MonoBehaviour
{
    private void Start()
    {
        AudioManager.instance.Play2D("DeathAudio");

        Destroy(gameObject, 3);
    }
}

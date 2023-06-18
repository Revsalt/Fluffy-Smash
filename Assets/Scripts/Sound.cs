using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;

    public bool randomPlay = true;
    [HideInInspector]public int playIndex = 0;
    public AudioClip[] clips;

    [Range(0f , 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;
    [Range(5, 30)]
    public float maxDistance = 10;
    public bool randompitch = false;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}

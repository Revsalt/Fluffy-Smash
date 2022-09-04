using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public partial class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;
    //AudioManager

    void Awake()
    {

        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);


        Play2D("ThemeSong");
    }

    public void Play(string name , Vector3 pos , Transform childOf)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return;
        }

        GameObject g = new GameObject("Sound" + UnityEngine.Random.Range(0, 8000));
        g.transform.position = pos;

        if (childOf != null)
            g.transform.SetParent(childOf);

        AudioSource As = g.AddComponent<AudioSource>();

        As.volume = s.volume;
        As.pitch = s.pitch;
        As.loop = s.loop;
        As.clip = s.clips[UnityEngine.Random.Range(0, s.clips.Length)];

        As.spatialBlend = 1;
        As.rolloffMode = AudioRolloffMode.Linear;
        As.maxDistance = s.maxDistance;

        if (s.randompitch)
            As.pitch = UnityEngine.Random.Range(.9f, 1.1f);

        As.Play();

        Destroy(As.gameObject, As.clip.length);
    }

    public void Play2D(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return;
        }

        GameObject g = new GameObject("Sound" + UnityEngine.Random.Range(0, 8000));

        AudioSource As = g.AddComponent<AudioSource>();

        As.volume = s.volume;
        As.pitch = s.pitch;
        As.loop = s.loop;
        As.clip = s.clips[UnityEngine.Random.Range(0, s.clips.Length)];

        if (s.randompitch)
            As.pitch = UnityEngine.Random.Range(.9f, 1.1f);

        As.Play();

        if (!As.loop) Destroy(As.gameObject, As.clip.length);

    }

    //this addition to the code was made by me, the rest was from Brackeys tutorial
    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        s.source.Stop();
    }
}
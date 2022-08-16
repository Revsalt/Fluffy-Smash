using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] AudioClip[] AudioFootSteps;
    [SerializeField] AudioClip[] AudioLand;
    [SerializeField] AudioClip AudioDash;
    [SerializeField] AudioClip AudioAirSwoosh;
    public void playSound(AudioClip ac)
    {
        GameObject ad = new GameObject("Sound");
        ad.transform.position = transform.position;

        ad.AddComponent<AudioSource>();
        ad.GetComponent<AudioSource>().clip = ac;
        ad.GetComponent<AudioSource>().volume = Random.Range(0.2f, 0.7f);
        ad.GetComponent<AudioSource>().reverbZoneMix = 2;
        ad.GetComponent<AudioSource>().Play();
        Destroy(ad, 1);
    }

    public void PlayDashSound()
    {
        GameObject ad = new GameObject("Sound");
        ad.transform.position = transform.position;

        ad.AddComponent<AudioSource>();
        ad.GetComponent<AudioSource>().clip = AudioDash;
        ad.GetComponent<AudioSource>().volume = Random.Range(0.2f, 0.7f);
        ad.GetComponent<AudioSource>().pitch = Random.Range(.9f, 1.1f);
        ad.GetComponent<AudioSource>().reverbZoneMix = 0;
        ad.GetComponent<AudioSource>().Play();
        Destroy(ad, 1);
    }

    public void PlayAirSwooshSound()
    {
        playSound(AudioAirSwoosh);
    }

    public void playSoundWalkFootStep()
    {
        
        GameObject ad = new GameObject("Sound" + Random.Range(1, 4000));
        ad.transform.position = transform.position;

        ad.AddComponent<AudioSource>();
        ad.GetComponent<AudioSource>().clip = AudioFootSteps[Random.Range(0 , AudioFootSteps.Length)];
        ad.GetComponent<AudioSource>().volume = Random.Range(0.2f, 0.7f);
        ad.GetComponent<AudioSource>().pitch = Random.Range(.9f, 1.1f);
        ad.GetComponent<AudioSource>().reverbZoneMix = 2;
        ad.GetComponent<AudioSource>().Play();
        Destroy(ad, 1);
    }

    public void playSoundLand()
    {
        GameObject ad = new GameObject("Sound" + Random.Range(1,4000));
        ad.transform.position = transform.position;

        ad.AddComponent<AudioSource>();
        ad.GetComponent<AudioSource>().clip = AudioLand[Random.Range(0, AudioLand.Length)];
        ad.GetComponent<AudioSource>().volume = Random.Range(0.2f, 0.7f);
        ad.GetComponent<AudioSource>().pitch = Random.Range(.9f, 1.1f);
        ad.GetComponent<AudioSource>().reverbZoneMix = 2;
        ad.GetComponent<AudioSource>().Play();
        Destroy(ad, 1);
    }
}

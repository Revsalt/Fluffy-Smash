using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelAnimationSounds : MonoBehaviour
{
    [SerializeField] string CharacterName = "Character";
    public void playSound(string audioName)
    {
        AudioManager.instance.Play(audioName, transform.position);
    }

    public void PlayAirSwooshSound()
    {
        playSound("AirSwoosh");
    }

    public void playSoundWalkFootStep()
    {
        playSound(CharacterName + "FootSteps");
    }

    public void playSoundLand()
    {
        playSound(CharacterName + "Land");
    }
}

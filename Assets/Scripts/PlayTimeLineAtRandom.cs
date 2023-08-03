using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PlayTimeLineAtRandom : MonoBehaviour
{
    public PlayableDirector pd;

    private void Start()
    {
        StartCoroutine(startRand());
    }

    IEnumerator startRand()
    {
        yield return new WaitForSeconds(Random.Range(10, 30));
        pd.Play();
        StartCoroutine(startRand());
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cinemachine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Mirror;

public class beetcatin : PlayerController
{
    [SerializeField] float bpm; //over 200 fuckes stuff up
    [SerializeField] int beat = 1;
    [SerializeField] bool Onbeat,grooving;
    [SerializeField] UnityEvent OnStart, OnEnd;
    [SerializeField] Transform MusicalBlocksPos;
    [SerializeField] GameObject MusicalNoteBlock;
    void Start()
    {

        StartCoroutine(StartMetronome());
        ability0 = new Ability()
        {
            ability = delegate
            {
                if (Onbeat && movementSpeed <14)
                {
                    movementSpeed += 3.5f;
                }
                else movementSpeed = GetOriginalSpeeed();
                ability0.End.Invoke();
                GameObject TempBlock = Instantiate(MusicalNoteBlock, MusicalBlocksPos.position,Quaternion.identity,null);
                Destroy(TempBlock, 5f);
            }
            , coolDown = 0.2f
            ,events = new UnityEvent[2] { OnStart, OnEnd }
        };
    }
    
    // Update is called once per frame
    new void Update()
    {
        base.Update();               
    }
    
    IEnumerator StartMetronome()
    {
        Debug.Log("Tick");
        if (beat == 4) Onbeat = true;
        else Onbeat = false;   
        yield return new WaitForSeconds(60/bpm);        
        if (beat > 3)
        {
            beat = 1;
        }
        else
        {
            beat++;
        }
        StartCoroutine(StartMetronome());
    }
}

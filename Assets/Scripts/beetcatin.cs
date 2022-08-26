using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cinemachine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Mirror;
using TMPro;

public class beetcatin : PlayerController
{
    [SerializeField] float bpm; //over 200 fuckes stuff up
    [SerializeField] int beat = 1;
    [SerializeField] bool Onbeat,grooving;
    [SerializeField] UnityEvent OnStart, OnEnd;
    [SerializeField] Transform MusicalBlocksPos;
    [SerializeField] GameObject MusicalNoteBlock;
    [SerializeField] TextMeshProUGUI BeatCounter;
    [SerializeField] TextMeshProUGUI Speed;
    [SerializeField] Transform OutRayPos;
    [SerializeField] float ForceValue;
    [SerializeField] GameObject _Projectile;
    [SerializeField] Transform cameralocation;
    void Start()
    {
        StartCoroutine(StartMetronome());
        ability0 = new Ability()
        {
            ability = delegate
            {
                if (Onbeat && movementSpeed <20)
                {
                    movementSpeed += 2.5f;
                }
                else if(!Onbeat) movementSpeed = GetOriginalSpeeed();
                ability0.End.Invoke();
                //GameObject TempBlock = Instantiate(MusicalNoteBlock, transform.position - Vector3.up + moveDirection * movementSpeed *0.2f,Quaternion.identity,null);
                //Destroy(TempBlock, 5f);
                GameObject projectile = Instantiate(_Projectile, OutRayPos.position,OutRayPos.rotation,null);
                projectile.GetComponent<MusicalProjectile>().Playerobject = this.gameObject;
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
    public void Force(Vector3 dir,float force)
    {
        AddImpact(dir, force * ForceValue, false);
    }
    IEnumerator chkmovement()
    {
        yield return new WaitForSeconds(3f);
        movementSpeed = GetOriginalSpeeed();
    }
    IEnumerator StartMetronome()
    {
        Debug.Log("Tick");
        if (beat == 4 || beat == 2) Onbeat = true;
        else Onbeat = false;
        BeatCounter.text = (beat + "/4");
        Speed.text = (movementSpeed.ToString());
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

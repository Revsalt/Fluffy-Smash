using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cinemachine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;
using TMPro;
using Unity.Mathematics;
using UnityEngine.SocialPlatforms;
using Random = Unity.Mathematics.Random;

public class beetcatin : PlayerController
{
    [Header("Beat System")]

    [SerializeField] float bpm;
    [SerializeField] int beat = 1;
    [SerializeField] float beatErorrMargin = .2f;
    private bool Onbeat;
    [SerializeField] Image bpm_image;
    [SerializeField] Slider bpm_slider_1;
    [SerializeField] Slider bpm_slider_2;

    [Header("ObjRef & TransRef")]
    [SerializeField] Transform OutRayPos;

    [Header("DrumAttack")]
    [SerializeField] GameObject KnockUpRing;
    int drumHitCount = 0;
    int lastBeat = 0;
    
    [Header("CymbalAttack")]
    [SerializeField] GameObject cymbal;

    [Header("PianoAttack")] [SerializeField]
    private GameObject PianoPrefab;
    [SerializeField] AnimationCurve PianoYvalue;
    [SerializeField] private float PianoSpeed = 50f;
    [SerializeField] private AudioClip Piano_Crash_sound;


    [Header("ParticleSystem")]
    [SerializeField] ParticleSystem trumpetBoostPS;

    [Header("Character Balance")]
    [SerializeField] float boostForce = 10;
    [SerializeField] int boostAmount = 3;

    public override void InitializeAbilities()
    {
        ability0.ability = delegate // trumpet boost
        {
            if (!isLocalPlayer)
            {
                StartCoroutine(TrumpetBoost());
                ability0.End.Invoke();
                return;
            }

            if (Onbeat) movementSpeed = Mathf.Clamp(movementSpeed + 2.5f , GetOriginalSpeeed(), 20);
            else movementSpeed = Mathf.Clamp(movementSpeed - 5f , GetOriginalSpeeed() , 20);

            if (boostAmount > 0)
            {
                StartCoroutine(TrumpetBoost());
                boostAmount--;
            }
            else
            {
                ability0.End.Invoke();
            }

        };

        ability0.events = new UnityEvent[2] { new UnityEvent(), new UnityEvent() };

        ability1.ability = delegate
        {
            if (!isLocalPlayer) return;

            if (Onbeat && beat != lastBeat)
            {
                lastBeat = beat;
                drumHitCount++;

                switch (drumHitCount)
                {
                    case 1: animator.Play("StartDrum"); { CmdStartDrum(0, GetComponent<NetworkIdentity>()); } ability1.skipNextCoolDown = true; AudioManager.instance.Play("OrchestraDrums", transform.position, transform); break;
                    case 2: animator.Play("drum 2"); { CmdStartDrum(1, GetComponent<NetworkIdentity>()); } ability1.skipNextCoolDown = true; break;
                    case 3: animator.Play("drum 3"); { CmdStartDrum(2, GetComponent<NetworkIdentity>()); } drumHitCount = 0; ability1.skipNextCoolDown = false; lastBeat = 0; break;
                    default: break;
                }
            }
            else { drumHitCount = 0; ability1.skipNextCoolDown = false; lastBeat = 0; animator.Play("Idle"); Debug.Log("no"); }

            ability1.End.Invoke();
        };

        ability1.events = new UnityEvent[2] { new UnityEvent(), new UnityEvent() };

        ability_Attack = new Ability()
        {
            ability = delegate
            {
                if (isGroundeed())
                {
                    //@hilado
                    //drop piano ( on input hold should cast a circle displayed on the ground and when input up -> it drops the piano where the circle is dealing damage
                    //(use a cube for now))
                    StartCoroutine(PianoAttack());
                }
                else
                {
                    GameObject cym = Instantiate(cymbal, transform.position, Quaternion.identity);
                    cym.transform.forward = cineCamera.transform.forward;
                }

                ability_Attack.End.Invoke();
            }
            ,
            coolDown = 0.2f
            ,
            events = new UnityEvent[2] { GetComponent<Health>().StartAttack, GetComponent<Health>().EndAttack }

        };
    }

    void Start()
    {
        StartCoroutine(StartMetronome());
    }

    IEnumerator PianoAttack()
    {
        GameObject CrossPointer = Instantiate(Resources.Load("RoundMarker") as GameObject, new Vector3(0, -100, 0), Quaternion.identity, null);

        RaycastHit hit = new RaycastHit();
        Vector3 lastHitNormal = Vector3.zero;

        ZoomIn(true);
        for (float i = 0; Input.GetMouseButton(0); i += Time.deltaTime)
        {
            Physics.Raycast(cineCamera.transform.position, cineCamera.transform.forward, out hit, 40, layerMask);
            if (Vector3.Angle(hit.normal.normalized,Vector3.up) <= 30) //  hit.normal.y * 90f >= -20 && hit.normal.y * 90f == 0
            {
                CrossPointer.transform.position = hit.point;
                CrossPointer.transform.rotation = Quaternion.LookRotation(cineCamera.transform.forward);

                CrossPointer.SetActive(true);
                lastHitNormal = hit.normal;
            }
            else
                CrossPointer.SetActive(false);

            yield return null;
        }
        ZoomIn(false);
        if (Vector3.Angle(lastHitNormal,Vector3.up) <= 30 && CrossPointer.activeSelf)
        {
            GameObject Piano = Instantiate(PianoPrefab, new Vector3(CrossPointer.transform.position.x,CrossPointer.transform.position.y + 50,CrossPointer.transform.position.z), quaternion.identity, null);
            float animationTime = 0;
            animationTime += Time.deltaTime;
            while (Vector3.Distance(Piano.transform.position,hit.point) > 0.5f)
            {
                Piano.transform.position -= new Vector3(0, Piano.transform.position.y, 0)  * PianoYvalue.Evaluate(animationTime) * PianoSpeed;
                yield return null;
            }
            Piano.GetComponent<Rigidbody>().AddForce(new Vector3(UnityEngine.Random.Range(-10f,10f),UnityEngine.Random.Range(0,10f),UnityEngine.Random.Range(-10f,10f)),ForceMode.Impulse);
            if(CrossPointer != null) { Destroy(CrossPointer); }
            GameObject Crack = Instantiate(Resources.Load("Crack") as GameObject, hit.point, Quaternion.Euler(90, 0, 0), null);
            Crack.GetComponent<AudioSource>().clip = Piano_Crash_sound;
            Crack.GetComponent<AudioSource>().Play();
            Destroy(Crack.gameObject, 5);

            ShakeCamera(6, .5f);
            yield return new WaitForSeconds(5f);
            if(Piano != null)Destroy(Piano);
        }
        ability_Attack.End.Invoke();
        yield return null;
    }
    IEnumerator TrumpetBoost()
    {
        AudioManager.instance.Play("trumpet1", transform.position, null);
        Vector3 dirc = cineCamera.transform.position + cineCamera.transform.forward * 10;
        animator.SetTrigger("IsPlatform_Attack");
        StartCoroutine(looktime());

        yield return new WaitForSeconds(.1f);

        trumpetBoostPS.Play();
        ShakeCamera(1.5f, .2f);
        ResetPlayerVelocity();
        AddImpact(cineCamera.transform.forward, boostForce, false);

        ability0.End.Invoke();

        IEnumerator looktime()
        {
            for (float y = 0; y < .5f; y += Time.deltaTime)
            {
                playerModel.transform.LookAt(cineCamera.transform.position - cineCamera.transform.forward * 10);
                yield return null;
            }
        }

    }

    new void Update()
    {
        if (!isLocalPlayer) return;

        base.Update();
        if (isGroundeed())
        {
            boostAmount = 3;
        }
        bool isRunning = moveDirection != Vector3.zero;
        animator.SetBool("IsWalk", isRunning);
        animator.SetBool("IsJump", !isGroundeed());
        animator.SetFloat("runSpeed", movementSpeed / 7);
        if (characterController.velocity.magnitude > 10)
        {
            animator.SetFloat("playerMagnitude", Mathf.Lerp(animator.GetFloat("playerMagnitude") , 2 , 8 * Time.deltaTime));
        }
        else
        {
            animator.SetFloat("playerMagnitude", Mathf.Lerp(animator.GetFloat("playerMagnitude") , 0 , 8 * Time.deltaTime));
        }

        if (moveDirection != Vector3.zero)
        {
            playerModel.transform.forward = moveDirection;
        }

        if (movementSpeed > 7f)
        {
            movementSpeed -= Time.deltaTime;
        }

        if (Onbeat) { bpm_image.color = Color.green; } else { bpm_image.color = Color.red; }

    }

    public static Vector3 Bezier2(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        var clamped_t = Mathf.Clamp(t, 0, 1);

        float u = 1 - clamped_t;
        float tt = clamped_t * clamped_t;
        float uu = u * u;
        Vector3 p = uu * p0;
        p += 2 * u * clamped_t * p1;
        p += tt * p2;

        return p;
    }

    IEnumerator StartMetronome()
    {
        if (!isLocalPlayer) yield break;

        for (float i = 0; i < (60 / bpm) - beatErorrMargin; i += Time.deltaTime)
        {
            bpm_slider_1.value = i / ((60 / bpm) - beatErorrMargin);
            bpm_slider_2.value = i / ((60 / bpm) - beatErorrMargin);
            yield return null;
        }

        Onbeat = true;

        AudioManager.instance.Play("HiHat", transform.position, transform);
        yield return new WaitForSeconds(beatErorrMargin);

        if (beat == 4)
        {
            beat = 1;
        }
        else { beat++; }

        Onbeat = false;
        StartCoroutine(StartMetronome());
    }

    #region DrumNetworks

    [Server]
    void SpawnKnockUpRing(float m, NetworkIdentity ntd)
    {
        StartCoroutine(delay());

        IEnumerator delay()
        {
            yield return new WaitForSeconds(.2f);
            GameObject g = Instantiate(KnockUpRing, transform.position, Quaternion.identity);
            g.GetComponent<KnockUpRing>().knockUpCaster = ntd;
            g.GetComponent<KnockUpRing>().sizeMultiplier = m;
            NetworkServer.Spawn(g);

            StartCoroutine(DestroyAfterFive(g));
        }

        IEnumerator DestroyAfterFive(GameObject k)
        {
            yield return new WaitForSeconds(5);
            NetworkServer.Destroy(k);
        }
    }

    [Command(requiresAuthority = false)]
    void CmdStartDrum(int whichDrum, NetworkIdentity ntd)
    {
        beetcatin bt = ntd.GetComponent<beetcatin>();
        switch (whichDrum)
        {
            case 0: bt.SpawnKnockUpRing(1.5f, ntd); break;
            case 1: bt.SpawnKnockUpRing(3f, ntd); break;
            case 2: bt.SpawnKnockUpRing(5f, ntd); break;
        }
    }

    #endregion

}

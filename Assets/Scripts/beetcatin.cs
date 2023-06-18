using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cinemachine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;
using TMPro;

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

    [SerializeField] int drumHitCount = 0;
    [SerializeField] int lastBeat = 0;

    [Header("RingHolo")]

    int HoloLv = 1;
    [SerializeField] GameObject HoloRingObj;
    [SerializeField] float lerpduration = 500f;
    [SerializeField] float ScaleMulti = 1f;
    [SerializeField] UnityEvent TagPing;

    [Header("ParticleSystem")]
    [SerializeField] ParticleSystem trumpetBoostPS;

    [Header("Character Balance")]
    [SerializeField] float boostForce = 10;
    [SerializeField] int boostAmount = 3;

    void Start()
    {
        StartCoroutine(StartMetronome());

        ability0.ability = delegate // trumpet boost
        {
            if (!isLocalPlayer)
            {
                StartCoroutine(TrumpetBoost());
                ability0.End.Invoke();
                return;
            }

            if (Onbeat && movementSpeed < 20)
            {
                Debug.Log("Boost");
                movementSpeed += 2.5f;
            }
            else if (!Onbeat)
            {
                movementSpeed = GetOriginalSpeeed();
            }

            if (boostAmount > 0)
            {
                StartCoroutine(TrumpetBoost());
                boostAmount--;
                ability0.skipNextCoolDown = true;

                if (boostAmount == 0)
                {
                    ability0.skipNextCoolDown = false;
                    boostAmount = 3;
                }
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
                ability_Attack.End.Invoke();
            }
            ,
            coolDown = 0.2f
            ,
            events = new UnityEvent[2] { GetComponent<Health>().StartAttack, GetComponent<Health>().EndAttack }

        };
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
        AddImpact(-cineCamera.transform.forward, boostForce, false);

        ability0.End.Invoke();

        IEnumerator looktime()
        {
            for (float y = 0; y < .5f; y += Time.deltaTime)
            {
                playerModel.transform.LookAt(dirc);
                yield return null;
            }
        }

    }

    new void Update()
    {
        if (!isLocalPlayer) return;

        base.Update();

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

        HoloRingObj.transform.Rotate(Vector3.up * 50 * Time.deltaTime * HoloLv, Space.World);

        if (movementSpeed > 7f)
        {
            movementSpeed -= Time.deltaTime;
        }

        if (Onbeat) { bpm_image.color = Color.green; } else { bpm_image.color = Color.red; }

    }

    IEnumerator HoloRing(int level)
    {
        if (level >= 2)
        {
            ScaleMulti += 2;
        }
        else if (level == 1)
        {
            ScaleMulti = 1f;
        }

        for (float i = 0; i < lerpduration; i += Time.deltaTime)
        {
            HoloRingObj.transform.localScale = Vector3.Lerp(HoloRingObj.transform.localScale, Vector3.one * ScaleMulti, 5 * Time.deltaTime);
            yield return null;
        }

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
        GameObject g = Instantiate(KnockUpRing, transform.position, Quaternion.identity);
        g.GetComponent<KnockUpRing>().knockUpCaster = ntd;
        g.GetComponent<KnockUpRing>().sizeMultiplier = m;
        NetworkServer.Spawn(g);

        StartCoroutine(DestroyAfterFive(g));

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

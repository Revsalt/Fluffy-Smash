using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using Mirror;
using UnityEngine.EventSystems;
using UnityEngine.Animations.Rigging;

public class HoodieCat : PlayerController
{
    [Header("hitGround")]
    [SerializeField] UnityEvent HitGroundNormal;
    [Header("inAir")]
    [SerializeField] UnityEvent inAirStart;
    [SerializeField] UnityEvent inAirEnd;
    [Header("inTeleport")]
    [SerializeField] UnityEvent StartTeleport;
    [SerializeField] UnityEvent EndTeleport;
    [Header("inStun")]
    [SerializeField] UnityEvent StartStun;
    [SerializeField] UnityEvent EndStun;
    [SerializeField] float stunRadius;
    [Header("Other")]
    [SerializeField] CinemachineVirtualCamera zoomCamera;
    [SerializeField] Transform pointerCastPosition , loliPopParent;
    [SerializeField] GameObject pointer, LoliPop;
    [SerializeField] GameObject[] modelHidden, modelNormal;
    [SerializeField] ParticleSystem movementParticleSystem;
    [SerializeField] GameObject playerModelIkTarget;
    [SerializeField] Rig rigWeight;
    [SerializeField] GameObject lolipopText;

    bool canBringLoliPop = true;
    bool loliPopTime = false;
    Animator animator;
    float oldGravity;

    private void Start()
    {
        animator = playerModel.GetComponentInChildren<Animator>();

        oldGravity = gravity;

        onJump += delegate { Debug.Log("Jump"); };

        ability0 = new Ability()
        {
            ability = delegate
            {
                StartCoroutine(AttackSequence2());
            },
            coolDown = 1, // 8
            events = new UnityEvent[2] { StartStun, EndStun }
        };

        ability1 = new Ability()
        {
            ability = delegate
            {
                StartCoroutine(AttackSequence1());
            },
            coolDown = 1f,
            events = new UnityEvent[2] { StartTeleport, EndTeleport }
        };


        TagLogic taglogic = GetComponent<TagLogic>();

        ability_tag = new Ability()
        {
            ability = delegate
            {
                StartCoroutine(AttackSquence0());
            },
            coolDown = 5f,
            events = new UnityEvent[] { taglogic.StartTag , taglogic.EndTag }
        };
    }

    float weight = 0;
    bool ranLandFunction = false;

    private void Update()
    {
        //Handling Ik

        lolipopText.SetActive(!HasLoliPop());

        if (playerModelIkTarget.transform.localPosition.z < -2)
            weight = 1 + Mathf.RoundToInt(playerModelIkTarget.transform.localPosition.z);
        else
            weight = 1;

        rigWeight.weight = weight;

        #region OnLand
        if (!isGroundeed())
        {
            inAirStart.Invoke();

            if (!ranLandFunction)
                StartCoroutine(Land());

            IEnumerator Land()
            {
                ranLandFunction = true;

                for (float i = 0; !isGroundeed(); i += Time.deltaTime)
                {
                    yield return null;
                }

                inAirEnd.Invoke();
                HitGroundNormal.Invoke();
                ShakeCamera(1, .2f);

                ranLandFunction = false;
            }
        }
        #endregion

        var mps = movementParticleSystem.emission;
        mps.rateOverDistance = Mathf.Lerp(mps.rateOverDistance.constant, movementSpeed / 1.2f, 5 * Time.deltaTime);

        ParticlesSystemEnabled(movementParticleSystem, isGroundeed());

        if (!isLocalPlayer)
            return;

        base.Update();

        animator.SetBool("isMove", moveDirection != Vector3.zero);
        if (HasLoliPop())
        {
            animator.SetFloat("hasLoliPop", 1);
            movementSpeed = GetOriginalSpeeed();
        }
        else
        {
            animator.SetFloat("hasLoliPop", 0);
            movementSpeed = GetOriginalSpeeed() * 2;
        }
        animator.SetFloat("runSpeed", movementSpeed / 7);

        playerModel.transform.LookAt(playerModel.transform.position + moveDirection);

        animator.SetBool("isJump", !isGroundeed());

        if (HasLoliPop())
            jumpHeight = GetOriginalJumpHeight();
        else 
            jumpHeight = GetOriginalJumpHeight() + GetOriginalJumpHeight() / 1.4f; 

        if (Input.GetMouseButtonDown(2) && !GetDisableInput() && !GetIsAnyAbilityInPorgress() && canBringLoliPop)
        {
            if (HasLoliPop())
                CmdDrop(GetComponent<NetworkIdentity>() , false);
            else
                CmdPickUp(!(Vector3.Distance(transform.position, LoliPop.transform.position) > 3) , GetComponent<NetworkIdentity>());
        }

        if (!HasLoliPop() && !loliPopTime)
        {
            loliPopTime = true;
            StartCoroutine(_LoliPopTime());
            IEnumerator _LoliPopTime()
            {
                for (float i = 0; !HasLoliPop(); i += Time.deltaTime)
                {
                    movementSpeed = Mathf.Clamp(movementSpeed - i, 2 , 20);               
                    yield return null;
                }

                loliPopTime = false;
            }
        }

        if (Camera.main)
        {
            Vector3 pos = playerModel.transform.position + Camera.main.transform.forward * 10;
            playerModelIkTarget.transform.position = pos;
        }
    }

    #region Drop
    [Command]
    void CmdDrop(NetworkIdentity ntd, bool onlyUnparent)
    {
        Drop(ntd , onlyUnparent);
        RpcDrop(ntd , onlyUnparent);
    }

    [ClientRpc]
    void RpcDrop(NetworkIdentity ntd, bool onlyUnparent)
    {
        Drop(ntd , onlyUnparent);
    }

    public void Drop(NetworkIdentity ntd , bool onlyUnparent)
    {
        GameObject lolipop_ = ntd.GetComponent<HoodieCat>().LoliPop;

        lolipop_.transform.SetParent(null);
        if (onlyUnparent)
            return;

        lolipop_.GetComponent<Rigidbody>().isKinematic = false;
        lolipop_.GetComponent<CapsuleCollider>().enabled = true;
    }
    #endregion

    #region PickUp
    [Command]
    void CmdPickUp(bool Teleport , NetworkIdentity ntd)
    {
        PickUp(Teleport , ntd);

        RpcPickUp(Teleport , ntd);
    }

    [ClientRpc]
    void RpcPickUp(bool Teleport, NetworkIdentity ntd)
    {
        PickUp(Teleport, ntd);
    }

    void PickUp(bool Teleport, NetworkIdentity ntd)
    {
        GameObject lolipop_ = ntd.GetComponent<HoodieCat>().LoliPop;

        StartCoroutine(delay());

        IEnumerator delay()
        {
            canBringLoliPop = false;

            lolipop_.gameObject.layer = LayerMask.NameToLayer("Default");
            lolipop_.GetComponent<CapsuleCollider>().enabled = false;
            lolipop_.GetComponent<Rigidbody>().isKinematic = true;

            if (!Teleport)
            {

                for (float i = 0; i <= 3; i += Time.deltaTime)
                {
                    foreach (var item in lolipop_.GetComponentsInChildren<MeshRenderer>())
                    {
                        item.material.SetFloat("Vector1_4A95455B", i);
                    }

                    if (i > .5f)
                        lolipop_.GetComponent<CapsuleCollider>().enabled = false;

                    yield return null;
                }

            }

            lolipop_.transform.SetParent(ntd.GetComponent<HoodieCat>().loliPopParent);
            lolipop_.transform.localPosition = Vector3.zero;
            lolipop_.transform.localRotation = Quaternion.identity;

            canBringLoliPop = true;

            if (Teleport)
                yield break;

            for (float i = 1; i >= -1; i -= Time.deltaTime)
            {
                foreach (var item in lolipop_.GetComponentsInChildren<MeshRenderer>())
                {
                    item.sharedMaterial.SetFloat("Vector1_4A95455B", i);
                }
                yield return null;
            }
        }
    }

    #endregion

    public void ZoomCamera(bool b)
    {
        if (b)
            zoomCamera.Priority = 2;
        else
            zoomCamera.Priority = 0;

        pointer.SetActive(b);
    }

    bool forceSmashLand = false;

    IEnumerator AttackSquence0()
    {
        if (!HasLoliPop())
        {
            ability_tag.skipNextCoolDown = true;
            ability_tag.End.Invoke();
            yield break;
        }

        playerModel.transform.rotation = Quaternion.LookRotation(new Vector3(pointerCastPosition.transform.forward.x, 0, pointerCastPosition.transform.forward.z));
        DisableInput(true);
        animator.SetBool("isAttackJump", true);

        yield return new WaitForSeconds(.25f);

        ResetPlayerVelocity();
        ShakeCamera(2, 0.4f);
        AudioManager.instance.Play("HoodieCatOffTheGround", transform.position , null);
        AddImpact(Vector3.up, 100, false);
        gravity = 0.1f;
        ZoomCamera(true);

        for (float i = 0; i < 1.5f; i += Time.deltaTime)
        {
            playerModel.transform.LookAt(playerModel.transform.position + pointerCastPosition.transform.forward * 10);

            if (isLocalPlayer && Input.GetMouseButtonDown(0)) {CmdForceStartSmashLand(GetComponent<NetworkIdentity>()); break;}
            if (!isLocalPlayer && forceSmashLand) { forceSmashLand = false; break; }

            yield return null;
        }

        GetComponent<TagLogic>().SetCanAttack(true);

        playerModel.transform.rotation = Quaternion.LookRotation(new Vector3(pointerCastPosition.transform.forward.x, 0, pointerCastPosition.transform.forward.z));
        ZoomCamera(false);
        gravity = oldGravity;
        AddImpact(pointerCastPosition.transform.forward, 500, false);

        for (float i = 0; !isGroundeed(); i += Time.deltaTime)
        {
            yield return null;
        }

        animator.SetBool("isAttackJump", false);
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5, layerMask);
        GameObject Crack = Instantiate(Resources.Load("Crack") as GameObject, hit.point, Quaternion.Euler(90, 0, 0), null);

        Destroy(Crack.gameObject, 5);

        ShakeCamera(6, .5f);
        DisableMovment(true);

        yield return new WaitForSeconds(.5f);

        GetComponent<TagLogic>().SetCanAttack(false);
        DisableMovment(false);
        DisableInput(false);

        ability_tag.End.Invoke();

        yield return null;
    }

    IEnumerator AttackSequence1()
    {
        if (!isLocalPlayer) yield break;

        if (!HasLoliPop())
        {
            ability1.skipNextCoolDown = true;
            ability1.End.Invoke();
            yield break;
        }

        TransclucentMode(true);
        GameObject CrossPointer = Instantiate(Resources.Load("RoundMarker") as GameObject, new Vector3(0, -100, 0), Quaternion.identity, null);

        RaycastHit hit = new RaycastHit();
        Vector3 lastHitNormal = Vector3.zero;

        for (float i = 0; !Input.GetKeyUp(KeyCode.LeftShift); i += Time.deltaTime)
        {
            Physics.Raycast(pointerCastPosition.position, pointerCastPosition.transform.forward, out hit, 40, layerMask);
            if (hit.collider)
            {
                CrossPointer.transform.position = hit.point;
                CrossPointer.transform.rotation = Quaternion.LookRotation(pointerCastPosition.transform.forward);

                CrossPointer.SetActive(true);
                lastHitNormal = hit.normal;
            }
            else
                CrossPointer.SetActive(false);

            yield return null;

        }

        if (lastHitNormal != Vector3.zero && CrossPointer.activeSelf)
        {
            DisableInput(true);
            DisableMovment(true);

            playerModel.transform.rotation = Quaternion.LookRotation(new Vector3(pointerCastPosition.transform.forward.x, 0, pointerCastPosition.transform.forward.z));
            animator.SetBool("isThrowLoliPop", true);
            CmdPlayAudio(transform.position);
            yield return new WaitForSeconds(0.2f);

            #region Moving The LoliPop
            TransclucentMode(false);
            CmdDrop(GetComponent<NetworkIdentity>(), true);
            Vector3 ThrowDirection = transform.right;
            Vector3 startpos_ = LoliPop.transform.position;
            Vector3 GoToPos_ = CrossPointer.transform.position + (hit.normal * 0.7f);

            for (float i = 0; Vector3.Distance(LoliPop.transform.position, GoToPos_) > 0; i += Time.deltaTime)
            {
                LoliPop.GetComponent<CapsuleCollider>().enabled = false;
                LoliPop.transform.position = Bezier2(startpos_, ((startpos_ + GoToPos_) / 2) + ThrowDirection * 2, GoToPos_,i * 2);
                LoliPop.transform.rotation = Quaternion.LookRotation((LoliPop.transform.position - GoToPos_).normalized);
                yield return null;
            }
            LoliPop.transform.rotation = Quaternion.LookRotation(lastHitNormal);
            AudioManager.instance.Play("HoodieCatLoliPopStickInWall", LoliPop.transform.position, null);
            #endregion

            #region Moving The Player
            Vector3 startpos;
            startpos = transform.position;

            DisableInput(false);
            animator.SetBool("isThrowLoliPop", false);
            Vector3 GoToPos = CrossPointer.transform.position + lastHitNormal + Vector3.up* 1;
            for (float i = 0; Vector3.Distance(transform.position, GoToPos) > .3f; i += Time.deltaTime)
            {
                transform.position = Bezier2(startpos, ((startpos + GoToPos) / 2) + Vector3.up * 5, GoToPos, i  * 2);
                yield return null;
            }
            #endregion

            LoliPop.GetComponent<CapsuleCollider>().enabled = true;

            DisableMovment(false);
            
        }

        animator.SetBool("isThrowLoliPop", false);
        Destroy(CrossPointer);
        TransclucentMode(false);
        ability1.End.Invoke();

        yield return null;
    }

    IEnumerator AttackSequence2()
    {
        //animator.SetBool("isStunAttack" , true);

        AudioManager.instance.Play("HoodieCatStunCircleGrow", LoliPop.transform.position, LoliPop.transform);

        yield return new WaitForSeconds(.6f);

        foreach (var item in FindObjectsOfType<TagLogic>())
        {
            if (!item.GetComponent<NetworkIdentity>().isLocalPlayer && Vector3.Distance(LoliPop.transform.position , item.transform.position) < stunRadius)
            {
                CmdStunPlayer(item.GetComponent<NetworkIdentity>());
            }
        }

        //animator.SetBool("isStunAttack", false);
        ability0.End.Invoke();
    }

    [Command]
    void CmdStunPlayer(NetworkIdentity ntd)
    {
         StunMe(ntd.connectionToClient);
    }

    [TargetRpc]
    void StunMe(NetworkConnection target)
    {
        StartCoroutine(delay());

        IEnumerator delay()
        {
            target.identity.GetComponent<PlayerController>().DisableMovment(true);
            yield return new WaitForSeconds(.7f);
            target.identity.GetComponent<PlayerController>().DisableMovment(false);
        }
    }

    [Command]
    void CmdForceStartSmashLand(NetworkIdentity ntd)
    {
        RpcForceSmashLand(ntd);
    }

    [ClientRpc]
    void RpcForceSmashLand(NetworkIdentity ntd)
    {
        ntd.GetComponent<HoodieCat>().forceSmashLand = true;
    }

    [Command]
    void CmdPlayAudio(Vector3 pos)
    {
        RpcPlayAudio(pos);
    }

    [ClientRpc]
    void RpcPlayAudio(Vector3 pos)
    {
        AudioManager.instance.Play("HoodieCatThrowLoliPop", pos, null);
    }

    bool HasLoliPop()
    {
        return LoliPop.transform.parent != null;
    }

    void ParticlesSystemEnabled(ParticleSystem ps, bool b)
    {
        if (b)
        {
            if (!ps.isPlaying)
                ps.Play();
        }
        else
        {
            if (ps.isPlaying)
                ps.Stop();
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

    void TransclucentMode(bool b)
    {
        foreach (var item in modelHidden)
        {
            item.SetActive(b);
        }

        foreach (var item in modelNormal)
        {
            item.SetActive(!b);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(LoliPop.transform.position, stunRadius);
    }
}

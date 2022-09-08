using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cinemachine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Mirror;

public class NinjaCat : PlayerController
{
    [Header("ParticleSystems")]
    [SerializeField] ParticleSystem movementParticleSystem;
    [SerializeField] ParticleSystem chargeParticleSystem;
    [Header("hitGround")]
    [SerializeField] UnityEvent HitGroundNormal;
    [SerializeField] UnityEvent HitGroundHarsh;
    [Header("inAir")]
    [SerializeField] UnityEvent inAirStart;
    [SerializeField] UnityEvent inAirEnd;
    [Header("inDash")]
    [SerializeField] UnityEvent inDashStart;
    [SerializeField] UnityEvent inDashEnd;
    [Header("Other")]
    [SerializeField] Vector3 wallOffset;
    [SerializeField] float wallJumpForce = 80;
    [SerializeField] float dashForce = 80;
    [SerializeField] GameObject playerModelIkTarget;
    [SerializeField] Rig rigWeight;
    [SerializeField] float StandDuration;
    [SerializeField] float distanceduration;
    Vector3 wallDirection = Vector3.zero;
    Animator animator;

    void Start()
    {
        animator = playerModel.GetComponentInChildren<Animator>();

        movementSpeed = GetOriginalSpeeed() * 2f;

        onJump += delegate {
            if (playerModel.GetComponentInChildren<ModelAnimationSounds>() != null) playerModel.GetComponentInChildren<ModelAnimationSounds>().PlayAirSwooshSound();
            animator.SetFloat("JumpNumber", Mathf.Round(Random.Range(0, 2)));
        };

        ability0 = new Ability()
        {
            ability = delegate
            {
                StartCoroutine(Dash());

                IEnumerator Dash()
                {
                    if (moveDirection != Vector3.zero)
                        AddImpact(moveDirection.normalized, dashForce , true);
                    else
                        AddImpact(Vector3.up, dashForce, false);

                    yield return new WaitForSeconds(0.1f);

                    ability0.End.Invoke();
                }
            },
            coolDown = 0.5f,
            events = new UnityEvent[2] { inDashStart, inDashEnd },
            abilityName = "QuickDash"
        };

        ability1 = new Ability()
        {
            ability = delegate
            {
                movementSpeed = GetOriginalSpeeed();
                Debug.Log("set slow");

                StartCoroutine(Sprint());

                IEnumerator Sprint()
                {
                    for (float i = 0; !Input.GetKeyUp(KeyCode.LeftShift); i += Time.deltaTime)
                    {
                        yield return null;
                    }

                    movementSpeed = GetOriginalSpeeed() * 2f;
                    Debug.Log("set fast");

                    ability1.End.Invoke();
                }
            },
            coolDown = 0,
            events = new UnityEvent[2] { new UnityEvent() , new UnityEvent()},
            abilityName = "Walk"
        };

        TagLogic taglogic = GetComponent<TagLogic>();

        ability_tag = new Ability()
        {
            ability = delegate
            {
                StartCoroutine(AttackSequence_tag());
            },
            coolDown = 2,
            events = new UnityEvent[] { taglogic.StartTag, taglogic.EndTag },
            abilityName = "SakuraBreeze"
        };
    }
    float weight = 0;
    bool ranHarshFallFunction = false;
    new void Update()
    {
        //Handling Ik

        if (playerModelIkTarget.transform.localPosition.z < 0)
            weight = 7 + Mathf.RoundToInt(playerModelIkTarget.transform.localPosition.z);
        else
            weight = 1;

        rigWeight.weight = weight;

        //ParticleSystem

        var mps = movementParticleSystem.main;
        mps.simulationSpeed = Mathf.Lerp(animator.GetFloat("runSpeed"), movementSpeed / 8, 5 * Time.deltaTime);

        ParticlesSystemEnabled(movementParticleSystem, isGroundeed());

        if (!isGroundeed())
        {
            inAirStart.Invoke();

            if (!ranHarshFallFunction)
                StartCoroutine(HarshFall());

            IEnumerator HarshFall()
            {
                bool harshFall = false;
                ranHarshFallFunction = true;

                for (float i = 0; !isGroundeed(); i += Time.deltaTime)
                {
                    if (i > 1)
                        harshFall = true;
                    yield return null;
                }

                inAirEnd.Invoke();
                HitGroundNormal.Invoke();
                ShakeCamera(1, .2f);
                if (harshFall)
                {
                    HitGroundHarsh.Invoke();
                    AddImpact(playerModel.transform.forward, 50, true);
                }

                ranHarshFallFunction = false;
            }
        }

        if (!isLocalPlayer)
            return;

        base.Update();

        //Animations

        bool isRunning = moveDirection != Vector3.zero;
        animator.SetBool("isRun", isRunning);
        animator.SetBool("isJump", !isGroundeed());
        animator.SetFloat("runSpeed", movementSpeed / 7);

        //wall running

        if (wallDirection != Vector3.zero && Input.GetButtonDown("Jump") && !GetDisableInput())
        {
            StartCoroutine(ChkIfJumped());

            DisableMovment(false);

            playerModel.GetComponentInChildren<ModelAnimationSounds>().PlayAirSwooshSound();
            animator.SetFloat("JumpNumber", Mathf.Round(Random.Range(0, 2)));
            animator.SetBool("isWallGrab", false);

            transform.rotation = Quaternion.Euler(Vector3.zero);

            if (Physics.Raycast(transform.position, -wallDirection, 10, layerMask))
            {
                if (isRunning)
                {
                    AddImpact(Vector3.up, wallJumpForce , true);
                    AddImpact(wallDirection, 60, true);
                }
                else
                {
                    AddImpact(wallDirection, 20, true);
                }
            }

            wallDirection = Vector3.zero;
        }

        if (Camera.main)
        {
            Vector3 pos = playerModel.transform.position + Camera.main.transform.forward * 10;
            playerModelIkTarget.transform.position = pos;
        }

        if (GetDisableInput()) return;

        if (wallDirection == Vector3.zero)
            playerModel.transform.LookAt(playerModel.transform.position + moveDirection);
        else
            playerModel.transform.localRotation = Quaternion.identity;
    }

    public override void OnControllerColliderHit(ControllerColliderHit hit)
    {
        base.OnControllerColliderHit(hit);

        if (wallDirection != Vector3.zero)
            return;

        if ((characterController.collisionFlags & CollisionFlags.Sides) != 0 && DistanceBetweenGround() > 1.5f && WallHeightIsEnough(hit.normal) && !isGroundeed())
        {
            playerModel.GetComponentInChildren<ModelAnimationSounds>().playSoundWalkFootStep();
            animator.SetBool("isWallGrab", true);

            DisableMovment(true);
            ResetPlayerVelocity();

            RaycastHit hitLine;
            Physics.Raycast(playerModel.transform.position, -hit.normal, out hitLine, 3, layerMask);
            transform.position = hitLine.point + new Vector3(hit.normal.x * wallOffset.x , hit.normal.y * wallOffset.y ,hit.normal.z * wallOffset.z);

            wallDirection = hit.normal;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, wallDirection);
        }
    }

    float originalgravity;

    IEnumerator AttackSequence_tag()
    {
        originalgravity = gravity;
        AudioSource audiosouce = null;
        
        DisableInput(true);
        gravity = 0; ResetPlayerVelocity();
        animator.SetBool("isPersonGrab" , true);
        chargeParticleSystem.Play();
        float DistanceDuration = 0.1f;
        for (float i = 0; Input.GetKey(KeyCode.Mouse0) && i < 1f; i += Time.deltaTime)
        {
            //audiosouce = AudioManager.instance.Play("SwordWindUp", transform.position, transform);
            playerModel.transform.LookAt(playerModelIkTarget.transform.position);
            var cps = chargeParticleSystem.main;
            cps.simulationSpeed = i + 1;
            DistanceDuration = i * 0.1f;
            yield return null;
        }

        //audiosouce.Stop();        
        chargeParticleSystem.Stop();
        chargeParticleSystem.Clear();

        HidePlayerModel(false);
        AddImpact(playerModel.transform.forward, 1400, false);
        ShakeCamera(3, 0.3f);

        GetComponent<TagLogic>().SetCanAttack(true);

        yield return new WaitForSeconds(DistanceDuration);
        AudioManager.instance.Play("SwordSlash", transform.position, transform);
        HidePlayerModel(true);
        ResetPlayerVelocity();
        AddImpact(playerModel.transform.forward, 70, false);

        yield return new WaitForSeconds(.5f);
        animator.SetBool("isPersonGrab", false);
        DisableInput(false);
        gravity = originalgravity;
        GetComponent<TagLogic>().SetCanAttack(false);

        ability_tag.End.Invoke();
    }

    void ParticlesSystemEnabled(ParticleSystem ps , bool b)
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

    void HidePlayerModel(bool visible)
    {
        foreach (var item in playerModel.transform.GetChild(0).GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            item.enabled = visible;
        }
    }

    private bool WallHeightIsEnough(Vector3 wallDirection)
    {
        bool condition = false;

        for (int i = 0; i < 2; i++)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position + new Vector3(0,i * .5f,0), -wallDirection, out hit , 1);
            //Debug.DrawRay(transform.position + new Vector3(0, i * .5f, 0), -wallDirection, Color.blue , 5);

            if (hit.collider != null)
            {
                condition = true;
            }
            else
            {
                condition = false;
                return false;
            }
        }

        return condition;
    }
}

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
    [SerializeField] ParticleSystem chargeParticleSystem;
    [SerializeField] ParticleSystem leafDashParticleSystem;
    [Header("inDash")]
    [SerializeField] UnityEvent inDashStart;
    [SerializeField] UnityEvent inDashEnd;

    [Header("CharacterBalancing")]
    [SerializeField] float wallJumpForce = 80;
    [SerializeField] float dashForce = 80;
    [SerializeField] float swordSlashDistance = 200;
    Vector3 wallDirection = Vector3.zero;

    public override void InitializeAbilities()
    {
        Health health = GetComponent<Health>();

        ability0.ability = delegate
        {
            if (moveDirection != Vector3.zero)
                AddImpact(moveDirection.normalized, dashForce, true);
            else
                AddImpact(Vector3.up, dashForce, false);

            ability0.End.Invoke();
        };

        ability0.events = new UnityEvent[2] { inDashStart, inDashEnd };

        ability1.ability = delegate
        {
            ability1.End.Invoke();
            return;

            movementSpeed = GetOriginalSpeeed();

            StartCoroutine(Sprint());

            IEnumerator Sprint()
            {
                for (float i = 0; !input_m.inputs[1]; i += Time.deltaTime)
                {
                    yield return null;
                }

                movementSpeed = GetOriginalSpeeed() * 2f;

                ability1.End.Invoke();
            }
        };

        ability1.events = new UnityEvent[2] { new UnityEvent(), new UnityEvent() };

        ability_Attack.ability = delegate
        {
            StartCoroutine(AttackSequence());
        };

        ability_Attack.events = new UnityEvent[] { health.StartAttack, health.EndAttack };
    }

    void Start()
    {
        resultantMovement += wallJumping;

        movementSpeed = GetOriginalSpeeed() * 2f;

        onJump += delegate
        {
            if (playerModel.GetComponentInChildren<ModelAnimationSounds>() != null) playerModel.GetComponentInChildren<ModelAnimationSounds>().PlayAirSwooshSound();
            animator.SetFloat("JumpNumber", Mathf.Round(Random.Range(0, 2)));
        };
    }

    void wallJumping()
    {
        if (wallDirection != Vector3.zero && (input_m.inputs[0] || input_m.inputs[3] && !GetDisableInput()))
        {
            OffWall(true);
        }
    }

    new void Update()
    {
        base.Update();

        //wall running

        if (GetDisableInput()) return;

        if (wallDirection == Vector3.zero)
            playerModel.transform.LookAt(playerModel.transform.position + moveDirection);
        else
            playerModel.transform.localRotation = Quaternion.identity;

        if (!isLocalPlayer)
            return;

        //Animations

        bool isRunning = moveDirection != Vector3.zero;
        animator.SetBool("isRun", isRunning);
        animator.SetBool("isJump", !isGroundeed());
        animator.SetFloat("runSpeed", movementSpeed / 7);
    }

    public void RollOnHarshFall()
    {
        AddImpact(playerModel.transform.forward, 50, true);
    }

    void OffWall(bool withDirectionalBoost)
    {
        DisableRotationInput(false);

        StopCoroutine(PlayerFallOnWall());

        ability_Attack.DisableAbility(false);
        ability1.DisableAbility(false);

        StartCoroutine(disableinputforabit());

        bool isRunning = moveDirection != Vector3.zero;

        StartCoroutine(ChkIfJumped());

        gravity = GetOriginalGraivty();
        movementSpeed = GetOriginalSpeeed() * 2;

        playerModel.GetComponentInChildren<ModelAnimationSounds>().PlayAirSwooshSound();
        animator.SetFloat("JumpNumber", Mathf.Round(Random.Range(0, 2)));
        animator.SetBool("isWallGrab", false);

        transform.rotation = Quaternion.Euler(Vector3.zero);

        if (Physics.Raycast(transform.position, -wallDirection, 10, layerMask))
        {
            if (isRunning && withDirectionalBoost)
            {
                ResetPlayerVelocity();
                AddImpact(Vector3.up, wallJumpForce, true);
                AddImpact(wallDirection + (moveDirection / 1.2f), 60, true);
            }
            else
            {
                ResetPlayerVelocity();
                AddImpact(wallDirection, 20, true);
            }
        }

        wallDirection = Vector3.zero;

        IEnumerator disableinputforabit()
        {
            DisableInput(true);
            yield return new WaitForSeconds(.2f);
            DisableInput(false);
        }
    }

    public override void OnControllerColliderHit(ControllerColliderHit hit)
    {
        base.OnControllerColliderHit(hit);

        if (wallDirection != Vector3.zero)
            return;

        if ((characterController.collisionFlags & CollisionFlags.Sides) != 0 
            && DistanceBetweenGround() > 1.5f && WallHeightIsEnough(hit.normal) && !isGroundeed() 
            && Vector3.Dot(Vector3.up, hit.normal) < 0.7f && Vector3.Dot(Vector3.up, hit.normal) > -0.7f && !GetIsAnyAbilityInPorgress())
        {
            ability_Attack.DisableAbility(true);
            ability1.DisableAbility(true);

            DisableRotationInput(true);

            StartCoroutine(PlayerFallOnWall());
            playerModel.GetComponentInChildren<ModelAnimationSounds>().playSoundWalkFootStep();
            animator.SetBool("isWallGrab", true);

            movementSpeed = 0;
            gravity = 0;
            ResetPlayerVelocity();

            RaycastHit hitLine;
            Physics.Raycast(playerModel.transform.position, -hit.normal, out hitLine, 3, layerMask);
            transform.position = hitLine.point + new Vector3(hit.normal.x, hit.normal.y, hit.normal.z);

            wallDirection = hit.normal;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, wallDirection);
        }
    }

    IEnumerator PlayerFallOnWall()
    {
        yield return new WaitForSeconds(.25f);

        if (wallDirection == Vector3.zero) yield break;

        gravity = -0.5f;

        while (!isGroundeed() && Physics.Raycast(transform.position , -wallDirection , 2f))
        {
            Debug.Log("OnWall");
            yield return null;
        }

        OffWall(false);
    }

    IEnumerator AttackSequence()
    {
        DisableInput(true);
        gravity = 0; ResetPlayerVelocity();
        animator.SetBool("isPersonGrab", true);
        chargeParticleSystem.Play();

        ZoomIn(true);
        AudioManager.instance.Play("NinjaCatSwordWhindUp", transform.position, null);
        float DistanceDuration = 0.1f;

        int s_tick = input_m.tick;
        while (input_m.tick < s_tick + 60 && input_m.inputs[2])
        {
            float i = (input_m.tick - s_tick);

            playerModel.transform.LookAt(piviot_M.transform.position + piviot_M.transform.forward * 1000);
            var cps = chargeParticleSystem.main;
            cps.simulationSpeed = i + 1;
            DistanceDuration = i;
            ShakeCamera(i, 1);

            Debug.Log(i);

            yield return null;
        }

        StartCoroutine(Dash(DistanceDuration));

        ZoomIn(false);

        // this section could still be adjusteed as to am sending the duration at which the player held the input , which is not ideal ,
        // the best way was to make sure both client and server hold the input for the same duration which is not easy and i hope to be able to
        // change that for the future
        /*
        if (isClient)
        {
            CmdSendDuration(DistanceDuration);
            StartCoroutine(Dash(DistanceDuration));
        }
        */
    }

    [Command]
    void CmdSendDuration(float d, NetworkConnectionToClient sender = null)
    {
        sender.identity.GetComponent<NinjaCat>();

        StartCoroutine(Dash(d));
    }

    #region DashReplication

    IEnumerator Dash(float duration)
    {
        Debug.Log(duration);
        chargeParticleSystem.Stop();
        chargeParticleSystem.Clear();
        leafDashParticleSystem.Play();

        HidePlayerModel(false);
        AddImpact(playerModel.transform.forward, swordSlashDistance, false);
        ShakeCamera(6, 0.3f);

        GetComponent<Health>().SetCanAttack(true);

        int c_tick = input_m.tick;
        while (input_m.tick < c_tick + duration)
        {
            yield return null;
        }

        AudioManager.instance.Play("NinjaCatSwordSlash", transform.position, transform);
        HidePlayerModel(true);
        ResetPlayerVelocity();
        AddImpact(playerModel.transform.forward, 70 , false);

        int c_tick2 = input_m.tick;
        while (input_m.tick > c_tick + .5f * 60)
        {
            yield return null;
        }

        animator.SetBool("isPersonGrab", false);
        DisableInput(false);
        gravity = GetOriginalGraivty();
        GetComponent<Health>().SetCanAttack(false);

        leafDashParticleSystem.Stop();
        ability_Attack.End.Invoke();
    }

    #endregion

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
            Physics.Raycast(transform.position + new Vector3(0, i * .5f, 0), -wallDirection, out hit, 1);
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

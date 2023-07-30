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
            StartCoroutine(Dash());

            IEnumerator Dash()
            {
                if (moveDirection != Vector3.zero)
                    AddImpact(moveDirection.normalized, dashForce, true);
                else
                    AddImpact(Vector3.up, dashForce, false);

                yield return new WaitForSeconds(0.1f);

                ability0.End.Invoke();
            }

        };

        ability0.events = new UnityEvent[2] { inDashStart, inDashEnd };

        ability1.ability = delegate
        {
            movementSpeed = GetOriginalSpeeed();

            StartCoroutine(Sprint());

            IEnumerator Sprint()
            {
                for (float i = 0; !Input.GetKeyUp(KeyCode.LeftShift); i += Time.deltaTime)
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
        movementSpeed = GetOriginalSpeeed() * 2f;

        onJump += delegate
        {
            if (playerModel.GetComponentInChildren<ModelAnimationSounds>() != null) playerModel.GetComponentInChildren<ModelAnimationSounds>().PlayAirSwooshSound();
            animator.SetFloat("JumpNumber", Mathf.Round(Random.Range(0, 2)));
        };
    }

    new void Update()
    {

        if (!isLocalPlayer)
            return;

        base.Update();

        //Animations

        bool isRunning = moveDirection != Vector3.zero;
        animator.SetBool("isRun", isRunning);
        animator.SetBool("isJump", !isGroundeed());
        animator.SetFloat("runSpeed", movementSpeed / 7);

        //wall running

        if (wallDirection != Vector3.zero && (Input.GetButtonDown("Jump") || Input.GetMouseButtonDown(1)) && !GetDisableInput())
        {
            OffWall(true);
        }

        if (GetDisableInput()) return;

        if (wallDirection == Vector3.zero)
            playerModel.transform.LookAt(playerModel.transform.position + moveDirection);
        else
            playerModel.transform.localRotation = Quaternion.identity;
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
        for (float i = 0; Input.GetMouseButton(0) && i < 1f; i += Time.deltaTime)
        {
            playerModel.transform.LookAt(cineCamera.transform.position + cineCamera.transform.forward * 1000);
            var cps = chargeParticleSystem.main;
            cps.simulationSpeed = i + 1;
            DistanceDuration = i * 0.1f;
            ShakeCamera(i, 1);
            yield return null;
        }

        ZoomIn(false);

        if (isLocalPlayer)
        {
            CmdSendChargeDuration(DistanceDuration);
            StartCoroutine(Dash(DistanceDuration));
        }
    }

    #region DashReplication

    [Command]
    void CmdSendChargeDuration(float distanceDuration, NetworkConnectionToClient sender = null)
    {
        RpcSendChargeDuration(distanceDuration, sender.identity);
    }

    [ClientRpc]
    void RpcSendChargeDuration(float distanceDuration, NetworkIdentity ntd)
    {
        if (!ntd.isLocalPlayer)
            ntd.GetComponent<NinjaCat>().StartCoroutine(Dash(distanceDuration));
    }

    IEnumerator Dash(float duration)
    {
        chargeParticleSystem.Stop();
        chargeParticleSystem.Clear();
        leafDashParticleSystem.Play();

        HidePlayerModel(false);
        AddImpact(playerModel.transform.forward, swordSlashDistance, false);
        ShakeCamera(6, 0.3f);

        GetComponent<Health>().SetCanAttack(true);

        yield return new WaitForSeconds(duration);
        AudioManager.instance.Play("NinjaCatSwordSlash", transform.position, transform);
        HidePlayerModel(true);
        ResetPlayerVelocity();
        AddImpact(playerModel.transform.forward, 70, false);

        yield return new WaitForSeconds(.5f);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cinemachine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Mirror;

public class Player : PlayerController
{
    [Header("ParticleSystems")]
    [SerializeField] ParticleSystem movementParticleSystem;
    [SerializeField] UnityEvent movementStart;
    [SerializeField] UnityEvent movementEnd;
    [Header("Dash")]
    [SerializeField] UnityEvent hitGroundStart;
    [SerializeField] UnityEvent hitGroundEnd;
    [Header("inAir")]
    [SerializeField] UnityEvent inAirStart;
    [SerializeField] UnityEvent inAirEnd;
    [Header("inDash")]
    [SerializeField] UnityEvent inDashStart;
    [SerializeField] UnityEvent inDashEnd;
    [Header("Other")]
    [SerializeField] Vector3 wallOffset;
    [SerializeField] CinemachineVirtualCamera cinemachineVirtualCamera;
    [SerializeField] AudioPlayer audioPlayer;
    [SerializeField] float wallJumpForce = 80;
    [SerializeField] float dashForce = 80;
    [SerializeField] Animator animator;
    [SerializeField] GameObject playermodelchild;
    [SerializeField] GameObject playerModelIkTarget;
    [SerializeField] Rig rigWeight;

    Vector3 wallDirection = Vector3.zero;
    float oldMovementSpeed = 0;
    bool canDash = true;

    void Start()
    {
        oldMovementSpeed = movementSpeed;
        movementSpeed = oldMovementSpeed * 2f;
    }

    float weight = 0;
    bool canBurstParticels = true;
    bool harshFall = false;
    float time = 0;
    bool canwalk = false;
    Bamboo lastbamboo = null;

    new void Update()
    {
        if (playerModelIkTarget.transform.localPosition.z < 0)
            weight = 7 + Mathf.RoundToInt(playerModelIkTarget.transform.localPosition.z);
        else
            weight = 1;

        rigWeight.weight = weight;

        Vector3 movementDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        bool isRunning = movementDirection != Vector3.zero;
        if (isLocalPlayer)
        {
            animator.SetBool("isRun", isRunning);
            animator.SetBool("isJump", !isGroundeed());
            animator.SetFloat("runSpeed", movementSpeed / 7);
        }

        var mps = movementParticleSystem.main;
        mps.simulationSpeed = Mathf.Lerp(animator.GetFloat("runSpeed"), movementSpeed / 8, 5 * Time.deltaTime);

        ParticlesSystemEnabled(movementParticleSystem, isGroundeed());
        if (!isGroundeed())
        {
            inAirStart.Invoke();
        }
        else
        {
            inAirEnd.Invoke();
        }

        if (isRunning)
        {
            if (canwalk)
            {
                movementStart.Invoke();
                canwalk = false;
            }
        }
        else
        {
            if (canwalk == false)
            {
                movementEnd.Invoke();
                canwalk = true;
            }
        }

        if (isGroundeed() && canBurstParticels)
        {
            hitGroundStart.Invoke();

            ShakeCamera(1, .2f);

            canBurstParticels = false;

            audioPlayer.playSoundLand();

            if (harshFall)
            {
                animator.SetTrigger("isRoll");
                AddImpact(playermodelchild.transform.forward, 50);
                harshFall = false;
            }

        }

        if (!isGroundeed() && wallDirection == Vector3.zero)
        {
            time -= Time.deltaTime;
            if (time < .1f)
            {
                harshFall = true;
            }

        }
        else
        {
            time = 1.2f;
        }

        if (!isGroundeed())
            canBurstParticels = true;

        if (!isLocalPlayer)
            return;

        base.Update();

        //sprinting

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            movementSpeed = oldMovementSpeed;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            movementSpeed = oldMovementSpeed * 2f;
        }

        if (Input.GetMouseButton(1) && !GetDisableMovement())
        {
            if (!canDash)
                return;

            audioPlayer.PlayDashSound();

            inDashStart.Invoke();
            canDash = false;

            Vector3 direc = piviot_M.transform.TransformDirection(movementDirection.normalized);
            direc.y = 0;

            if (isRunning)
                AddImpact(direc, dashForce);
            else
                AddImpact(Vector3.up, dashForce);

            animator.SetTrigger("isDash");

            StartCoroutine(ReChargeDash());

        }

        if (Input.GetKeyDown(KeyCode.Space) && isGroundeed())
        {
            audioPlayer.PlayAirSwooshSound();
            animator.SetFloat("JumpNumber", Mathf.Round(Random.Range(0, 2)));
        }

        //wall running

        if (wallDirection != Vector3.zero && Input.GetKeyDown(KeyCode.Space))
        {
            DisableMovment(false);

            audioPlayer.PlayAirSwooshSound();
            animator.SetFloat("JumpNumber", Mathf.Round(Random.Range(0, 2)));
            animator.SetBool("isWallGrab", false);
            animator.SetBool("isBambooGrab", false);
            if (lastbamboo != null)
            {
                lastbamboo.StartBounce();
                lastbamboo = null;
            }

            transform.rotation = Quaternion.Euler(Vector3.zero);

            if (Physics.Raycast(transform.position, -wallDirection, 10, layerMask))
            {
                if (isRunning)
                {
                    AddImpact(Vector3.up, wallJumpForce);
                    AddImpact(wallDirection, 60);
                }
                else
                {
                    AddImpact(wallDirection, 20);
                }
            }

            wallDirection = Vector3.zero;
        }

        Vector3 direction = transform.right * Input.GetAxisRaw("Horizontal") +
        transform.forward * Input.GetAxisRaw("Vertical");

        if (wallDirection == Vector3.zero)
            playermodelchild.transform.LookAt(playermodelchild.transform.position + direction);
        else
            playermodelchild.transform.localRotation = Quaternion.identity;

        if (Camera.main)
        {
            Vector3 pos = playerModel.transform.position + Camera.main.transform.forward * 10;
            playerModelIkTarget.transform.position = new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
        }
    }

    IEnumerator ReChargeDash()
    {      
        yield return new WaitForSeconds(.5f);
        inDashEnd.Invoke();
        canDash = true;
    }

    public void TaggingAnimation(bool t)
    {
        animator.SetBool("isPersonGrab",t);
    }

    public void ShakeCamera(float intensity , float time)
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
            cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        StartCoroutine(timer());

        IEnumerator timer()
        {
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
            yield return new WaitForSeconds(time);
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0;
        }
    }

    public override void OnControllerColliderHit(ControllerColliderHit hit)
    {
        base.OnControllerColliderHit(hit);

        if (wallDirection != Vector3.zero)
            return;

        if ((characterController.collisionFlags & CollisionFlags.Sides) != 0 && DistanceBetweenGround() > 1.5f && WallHeightIsEnough(hit.normal) && hit.normal.y * 90f >= -20)
        {
            audioPlayer.playSoundWalkFootStep();

            DisableMovment(true);
            ResetPlayerVelocity();

            RaycastHit hitLine;
            Physics.Raycast(playerModel.transform.position, -hit.normal, out hitLine, 3, layerMask);
            transform.position = hitLine.point + new Vector3(hit.normal.x * wallOffset.x , hit.normal.y * wallOffset.y ,hit.normal.z * wallOffset.z);

            wallDirection = hit.normal;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, wallDirection);

            if (hit.collider.tag != "Bamboo")
            {
                animator.SetBool("isWallGrab", true);
            }
            else
            {
                lastbamboo = hit.collider.GetComponent<Bamboo>();
                lastbamboo.StopBounce();
                animator.SetBool("isBambooGrab", true);
            }

            canDash = true;
        }
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

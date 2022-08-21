using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [HideInInspector]public CharacterController characterController;
    Vector3 playerVelocity = Vector3.zero;
    Vector3 impact = Vector3.zero;

    bool disableMovement = false;
    bool disableInput = false;
    float originalMovementSpeed = 0;

    CinemachineVirtualCamera cineCamera;

    //events
    public delegate void Jump();
    public event Jump onJump;
    public Ability ability0;

    [Header("Default")]
    [SerializeField] public GameObject playerModel;
    [SerializeField] public GameObject piviot_M;
    [Header("Camera")]
    [SerializeField]private float sensitvity = 100;
    [Header("Movement")]
    public float movementSpeed = 5;
    [SerializeField] private float slopeForce;
    [SerializeField] private float slopeForceRayLength;
    [Header("Jumping")]
    [SerializeField]private float jumpHeight = 5;
    public LayerMask layerMask = 5;
    [SerializeField] public float gravity = 5;
    [Header("Networks")]
    public GameObject[] Cameras;

    
    void OnValidate()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    private void Awake()
    {
        originalMovementSpeed = movementSpeed;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        piviot_M.transform.SetParent(null);

        foreach (var item in Cameras)
        {
            if (item.GetComponent<CinemachineVirtualCamera>())
            {
                cineCamera = item.GetComponent<CinemachineVirtualCamera>();
            }
        }
    }

    float rotX, rotY;
    [HideInInspector] public Vector3 moveDirection = Vector3.zero;
    public void Update()
    {
        if (!disableInput)
        {
            moveDirection = transform.right * Input.GetAxisRaw("Horizontal") +
                transform.forward * Input.GetAxisRaw("Vertical");
        } else { moveDirection = Vector3.zero; }

        //CameraPosistionAdjustment
        piviot_M.transform.position = transform.position;

        //Camera Movement

        rotX += Input.GetAxis("Mouse X") * sensitvity * Time.deltaTime;
        rotY += Input.GetAxis("Mouse Y") * sensitvity * Time.deltaTime;

        rotY = Mathf.Clamp(rotY, -80f, 80f);

        piviot_M.transform.localRotation = Quaternion.Euler(-rotY, rotX, 0f);

        //Movement

        if (disableMovement)
            return;

        if (moveDirection != Vector3.zero)
            transform.rotation = Quaternion.Euler(0,piviot_M.transform.eulerAngles.y , 0);

        //Movement and impact

        Vector3 Result = Vector3.zero;

        if (impact.magnitude > 0.2) Result += impact * Time.deltaTime;
         impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);
        

        if (characterController.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
        }

        Result += moveDirection * Time.deltaTime * movementSpeed;

        if (Input.GetButtonDown("Jump") && isGroundeed() && !GetDisableInput())
        {
            playerVelocity.y = 0;
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);

            //Callin the event for children classes
            onJump();
        }

        playerVelocity.y += gravity * Time.deltaTime * 3;
        characterController.Move(Result + (playerVelocity * Time.deltaTime));

        if ((moveDirection != Vector3.zero) && OnSlope())
        {
            characterController.Move(Vector3.down * characterController.height / 2 * slopeForce * Time.deltaTime);
        }

        //Abilities

        if (Input.GetMouseButton(1) && !GetDisableInput())
        {
            StartAbility(ability0);
        }

        // Debugging

        if (Input.GetKeyDown(KeyCode.P))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public float GetOriginalSpeeed()
    {
        return originalMovementSpeed;
    }

    public void SetPlayerPosition(Vector3 newPosition)
    {
        characterController.enabled = false;
        transform.position = newPosition;
        characterController.enabled = true;
    }

    public void DisableInput(bool enabled)
    {
        disableInput = enabled;
    }

    public void DisableMovment(bool enabled)
    {
        disableMovement = enabled;

        if (enabled)
            impact = Vector3.zero;
    }

    public bool GetDisableMovement()
    {
        return disableMovement;
    }

    public bool GetDisableInput()
    {
        return disableInput;
    }

    public float DistanceBetweenGround()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, -Vector3.up, out hit, layerMask, 99999);

        return Vector3.Distance(transform.position , hit.point);
    }

    public void AddImpact(Vector3 dir, float force)
    {
        dir.Normalize();
        if (dir.y < 0) dir.y = -dir.y; // reflect down force on the ground
        impact += dir.normalized * force / 3;
    }

    public void ResetPlayerVelocity()
    {
        playerVelocity = Vector3.zero;
        impact = Vector3.zero;
    }

    public bool isGroundeed()
    {
        return Physics.CheckSphere(transform.position - new Vector3(0, characterController.height / 2, 0), .4f, layerMask);
    }

    public void StartAbility(Ability abilityRef)
    {
        if (!abilityRef.canCast) return;

        StartCoroutine(CoolDown());

        IEnumerator CoolDown()
        {
            abilityRef.events[0].Invoke();
            abilityRef.ability.Invoke();
            abilityRef.canCast = false;

            yield return new WaitForSeconds(abilityRef.coolDown);

            abilityRef.canCast = true;
            abilityRef.events[1].Invoke();
        }
    }

    public void ShakeCamera(float intensity, float time)
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
            cineCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        StartCoroutine(timer());

        IEnumerator timer()
        {
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
            yield return new WaitForSeconds(time);
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0;
        }
    }

    private bool OnSlope()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, characterController.height / 2 * slopeForceRayLength))
            if (hit.normal != Vector3.up)
            {
                return true;
            }
        return false;
    }

    public virtual void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if ((characterController.collisionFlags & CollisionFlags.CollidedAbove) != 0)
        {
            AddImpact(Vector3.up, -10);
        }
    }
}

public class Ability
{
    public Action ability;
    public float coolDown;
    public UnityEvent[] events;

    public bool canCast = true;
}

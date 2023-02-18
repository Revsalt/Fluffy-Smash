using Cinemachine;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityStandardAssets.Effects;
using UnityEngine.InputSystem;

/// <summary>
/// for offline testing use the PlayerControllerOffline 
/// </summary>
public class PlayerController : NetworkBehaviour
{
    [HideInInspector] public CharacterController characterController;
    [HideInInspector] public Transform folllowTarget;
    Vector3 playerVelocity = Vector3.zero;
    Vector3 impact = Vector3.zero;
    float CoolDownReducer = 1f;

    bool disableMovement = false;
    bool disableInput = false;
    float originalMovementSpeed = 0;
    float originalJumpHeight = 0;
    Vector3 groundNormal = Vector3.zero;

    [HideInInspector]public CinemachineVirtualCamera cineCamera;
    [HideInInspector]public Animator animator;

    //events
    public delegate void Jump();
    public event Jump onJump;

    public Ability ability0, ability1, ability_Attack;

    [Header("Default")]
    [SerializeField] public GameObject playerModel;
    [SerializeField] public GameObject piviot_M;
    [Header("Camera")]
    public float sensitvity = 100;
    [Header("Movement")]
    bool HasJumped = true;
    public float movementSpeed = 5;
    [SerializeField] private float slopeForce;
    [SerializeField] private float slopeForceRayLength;
    [SerializeField] private float slideFriction;
    [Header("Jumping")]
    public float jumpHeight = 5;
    public LayerMask layerMask = 5;
    public float gravity = 5;
    [Header("Networks")]
    public GameObject[] Cameras;

    //GameObject platformMovingChild = null;

    bool HasJumpUp = false, HasSpeedUp = false, HasCoolUp = false;
    void OnValidate()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    private void Awake()
    {
        //platformMovingChild = new GameObject("platformMovingChild");

        //platformMovingChild.transform.SetParent(transform);
        //platformMovingChild.transform.position = transform.position;

        animator = playerModel.GetComponentInChildren<Animator>();
        folllowTarget = transform;
        originalJumpHeight = jumpHeight;
        originalMovementSpeed = movementSpeed;
        onJump += delegate { };
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        piviot_M.transform.SetParent(null);

        foreach (var item in Cameras)
        {
            if (item.GetComponent<CinemachineVirtualCamera>())
            {
                cineCamera = item.GetComponent<CinemachineVirtualCamera>();
                return;
            }
        }
    }

    Vector3 inputAxis = Vector3.zero;
    bool isJumpPressed = false;
    public void Move(InputAction.CallbackContext ctx)
    {
        var inputValue = ctx.ReadValue<Vector2>();
        inputAxis = new Vector3(inputValue.x, 0f, inputValue.y);
    }

    public void JumpInput(InputAction.CallbackContext ctx)
    {
       if (!ctx.performed) { return; }

        isJumpPressed = true;
    }

    private void LateUpdate()
    {
        //CameraPosistionAdjustment

        piviot_M.transform.position = folllowTarget.position;

        //Camera Movement

        rotX += Input.GetAxis("Mouse X") * sensitvity * Time.deltaTime;
        rotY += Input.GetAxis("Mouse Y") * sensitvity * Time.deltaTime;

        rotY = Mathf.Clamp(rotY, -80f, 80f);

        piviot_M.transform.localRotation = Quaternion.Euler(-rotY, rotX, 0f);

        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
        {
            sensitvity += 15;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
        {
            sensitvity -= 15;
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }

    float rotX, rotY;
    [HideInInspector] public Vector3 moveDirection = Vector3.zero;

    public void Update()
    {
        if (!disableInput)
        {
            moveDirection = new Vector3(cineCamera.transform.right.x , 0 , cineCamera.transform.right.z).normalized * inputAxis.x  +
                new Vector3(cineCamera.transform.forward.x, 0, cineCamera.transform.forward.z).normalized * inputAxis.z;
        }
        else { moveDirection = Vector3.zero; }

        //Movement

        if (disableMovement)
        {
            //Vector3 _translation = platformMovingChild.transform.position - transform.position;

            //characterController.Move(_translation);

            //platformMovingChild.transform.position = transform.position;

            return;
        }

        if (moveDirection != Vector3.zero)
            transform.rotation = Quaternion.Euler(0, piviot_M.transform.eulerAngles.y, 0);

        //Movement and impact

        Vector3 Result = Vector3.zero;

        if (impact.magnitude > 0.2) Result += impact * Time.deltaTime;
        impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);

        if (isGroundeed() && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
        }

        Result += moveDirection.normalized * Time.deltaTime * movementSpeed;

        if (isJumpPressed && isGroundeed() && !GetDisableInput())
        {
            playerVelocity.y = 0;
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);
            StartCoroutine(ChkIfJumped());
            //Callin the event for children classes
            onJump();
        }

        isJumpPressed = false;

        if (groundNormal != Vector3.zero && characterController.isGrounded)
        {
            AddImpact(new Vector3(groundNormal.y, -groundNormal.x, 0), (1f - groundNormal.y) * groundNormal.x * (1f - slideFriction), false);
            AddImpact(new Vector3(0, -groundNormal.z, groundNormal.y), (1f - groundNormal.y) * groundNormal.z * (1f - slideFriction), false);
        }

        //Vector3 translation = platformMovingChild.transform.position - transform.position;

        if (!characterController.isGrounded)
            playerVelocity.y += gravity * Time.deltaTime * 3;
        characterController.Move(Result + (playerVelocity * Time.deltaTime));

        //platformMovingChild.transform.position = transform.position;

        if ((moveDirection != Vector3.zero) && OnSlope() && HasJumped)
        {
            Debug.Log("on");
            characterController.Move(Vector3.down * characterController.height / 2 * slopeForce * Time.deltaTime);
        }

        //Abilities

        if (Input.GetMouseButtonDown(1) && !GetDisableInput())
        {
            StartAbility(0);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !GetDisableInput())
        {
            StartAbility(1);
        }

        if (Input.GetMouseButtonDown(0) && !GetDisableInput() && GetComponent<Health>().canInfluenceDamage)
        {
            StartAbility(2);
        }

    }

    public IEnumerator ChkIfJumped()
    {
        HasJumped = false;
        yield return new WaitForSeconds(0.2f);
        HasJumped = true;
    }

    private void OnGUI()
    {
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

    public float GetOriginalJumpHeight()
    {
        return originalJumpHeight;
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

        return Vector3.Distance(transform.position, hit.point);
    }

    public void AddImpact(Vector3 dir, float force, bool reflectOnGround)
    {
        dir.Normalize();
        if (reflectOnGround)
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

    bool abilityInProgress = false;
    public void StartAbility(int i)
    {
        Ability abilityRef = new Ability[3] { ability0, ability1, ability_Attack }[i];

        if (!abilityRef.canCast || abilityInProgress) return;

        CmdStartAbility(i, GetComponent<NetworkIdentity>());

        abilityRef.End += delegate
        {
            abilityRef.events[1].Invoke();
            abilityInProgress = false;
        };

        StartCoroutine(CoolDown());

        IEnumerator CoolDown()
        {
            abilityInProgress = true;
            abilityRef.events[0].Invoke();
            abilityRef.ability.Invoke();
            abilityRef.canCast = false;

            if (!abilityRef.skipNextCoolDown)
            {
                for (float z = 0; z < abilityRef.coolDown*CoolDownReducer; z += Time.deltaTime)
                {
                    abilityRef.coolDown_current_value = z;
                    yield return null;
                } 
            }
            else
                abilityRef.skipNextCoolDown = true;
            yield return null;

            abilityRef.canCast = true;
        }
    }


    void CmdStartAbility(int i, NetworkIdentity ntd)
    {
        if (isLocalPlayer)
            StartAbilityCmd(i, ntd);
    }

    [Command]
    void StartAbilityCmd(int i, NetworkIdentity ntd)
    {
        RpcStartAbility(i, ntd);
    }

    [ClientRpc]
    void RpcStartAbility(int i, NetworkIdentity ntd)
    {
        ntd.GetComponent<PlayerController>().StartAbility(i);
    }

    public bool GetIsAnyAbilityInPorgress()
    {
        return abilityInProgress;
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
            if (hit.normal != Vector3.up && hit.collider.tag != "Player")
            {
                return true;
            }
        return false;
    }

    public virtual void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //platformMovingChild.transform.parent = hit.transform;
        //platformMovingChild.transform.position = transform.position;

        Rigidbody rigidbody = hit.collider.attachedRigidbody;
        if (rigidbody != null)
        {
            Vector3 forcedirection = hit.gameObject.transform.position - transform.position;
            forcedirection.y = 0;
            forcedirection.Normalize();

            rigidbody.AddForceAtPosition((forcedirection * 25 / rigidbody.mass) * Time.deltaTime, transform.position, ForceMode.Impulse);
        }

        if ((characterController.collisionFlags & CollisionFlags.CollidedAbove) != 0)
        {
            AddImpact(Vector3.up, -1, true);
        }

        groundNormal = hit.normal;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;

        if (other.tag == "PowerUp")
        {           
            BasePowerUp OtherPowerup = other.GetComponent<PowerUp>().GetPowerUp();
            switch (OtherPowerup.PowerupEffect)
            {
                case BasePowerUp.effect.Speed:if (HasSpeedUp == false) { StartCoroutine(SpeedUp(OtherPowerup.EffectDuration)); } break;
                case BasePowerUp.effect.Jump:if (HasJumpUp == false) { StartCoroutine(JumpUp(OtherPowerup.EffectDuration)); } break;
                case BasePowerUp.effect.Cooldown: if (HasCoolUp == false) { StartCoroutine(CoolDownUp(OtherPowerup.EffectDuration));} break;
            }

            other.GetComponent<PowerUp>().NetworkDestroy();
        }
    }
    private IEnumerator SpeedUp(float duration)
    {
        HasSpeedUp = true;
        movementSpeed += 5f;
        yield return new WaitForSeconds(duration);
        HasSpeedUp = false;
        movementSpeed -= 5f;
    }
    private IEnumerator JumpUp(float duration)
    {
        HasJumpUp = true;
        jumpHeight += 5f;
        gravity = -5f;
        yield return new WaitForSeconds(duration);
        HasJumpUp = false;
        jumpHeight -= 5f;
        gravity = -9.8f;
    }

    private IEnumerator CoolDownUp(float duration)
    {
        HasCoolUp = true;
        CoolDownReducer = 0.5f;
        yield return new WaitForSeconds(duration);
        HasCoolUp = false;
        CoolDownReducer = 1f;
    }

    List<Coroutine> coroutines = new List<Coroutine>();
    GameObject pointer = null;
    public void ZoomIn(bool b)
    {
        if (!isLocalPlayer) return;

        foreach (var item in coroutines)
        {
            if (item != null)
                StopCoroutine(item);
        }

        float result = 0;

        if (b) { result = 1.7f; if (!pointer) { pointer = (GameObject)Instantiate(Resources.Load("PointerCanvas")); }  }
        else{ result = 4.5f; Destroy(pointer); }

        coroutines.Add(StartCoroutine(SetFOV(result)));


        IEnumerator SetFOV(float i)
        {
            CinemachineComponentBase componentBase = cineCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            if (componentBase is Cinemachine3rdPersonFollow)
            {
                for (float k = 0; Mathf.Round((componentBase as Cinemachine3rdPersonFollow).CameraDistance) != i; k += Time.deltaTime)
                {
                    (componentBase as Cinemachine3rdPersonFollow).CameraDistance = Mathf.Lerp((componentBase as Cinemachine3rdPersonFollow).CameraDistance, i, 7 * Time.deltaTime); // your value
                    yield return null;
                }
            }
            yield break;
        }

    }
}

public class Ability
{
    public Action ability;

    public float coolDown;
    public float coolDown_current_value = 0;

    public string abilityName = "NoName (please assign a name)";

    public UnityEvent[] events;
    public Action End;

    public bool canCast = true;
    public bool skipNextCoolDown = false;

}

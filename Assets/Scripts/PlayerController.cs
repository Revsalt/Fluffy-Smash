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
    bool disableRotationInput = false;
    float originalMovementSpeed = 0;
    float originalJumpHeight = 0;
    float originalGravity = 0;
    Vector3 groundNormal = Vector3.zero;

    [HideInInspector] public CinemachineVirtualCamera cineCamera;
    [HideInInspector] public Animator animator;

    //events
    public delegate void Jump();
    public event Jump onJump;

    [Header("Default")]
    [SerializeField] public GameObject playerModel;
    [SerializeField] public GameObject piviot_M;
    [Header("Camera")]
    public float sensitvity = 100;
    [Header("Movement")]
    bool HasJumped = true;
    public float movementSpeed = 5;
    //[SerializeField] private float slopeForce;
    //[SerializeField] private float slopeForceRayLength;
    //[SerializeField] private float slideFriction;
    [SerializeField] float airResistence;

    [Header("Jumping")]
    public float jumpHeight = 5;
    public LayerMask layerMask = 5;
    public float gravity = 5;
    [Header("Networks")]
    public GameObject[] Cameras;

    [Header("Ability Settings")]
    public Ability ability0;
    public Ability ability1, ability_Attack;

    bool HasJumpUp = false, HasSpeedUp = false, HasCoolUp = false;
    void OnValidate()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    private void Awake()
    {
        InitializeAbilities();

        airResistence *= TickRate.GetMinTimeBetweenTicks();

        Cameras[0].transform.parent.SetParent(null);

        animator = playerModel.GetComponentInChildren<Animator>();
        folllowTarget = transform;
        originalJumpHeight = jumpHeight;
        originalMovementSpeed = movementSpeed;
        originalGravity = gravity;
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

    public virtual void InitializeAbilities() {  }

    float rotX, rotY;
    [HideInInspector] public Vector3 moveDirection = Vector3.zero;
    public ClientInput input_m;

    public Action resultantMovement;

    public MovementResult ResultantMovement(ClientInput input)
    {
        input_m = input;

        resultantMovement.Invoke();

        piviot_M.transform.localRotation = input.cameraRotation;

        if (!disableInput)
        {
            moveDirection = new Vector3(folllowTarget.transform.right.x, 0, folllowTarget.transform.right.z).normalized * input.movementAxis.x +
                new Vector3(folllowTarget.transform.forward.x, 0, folllowTarget.transform.forward.z).normalized * input.movementAxis.y;
        }
        else { moveDirection = Vector3.zero; }

        //Movement and impact

        if (moveDirection != Vector3.zero && !disableRotationInput)
            transform.rotation = Quaternion.Euler(0, piviot_M.transform.eulerAngles.y, 0);

        if (isGroundeed())
        {
            airResistence = 6.2f;
        }
        else
        {
            airResistence = 0.8f;
        }

        Vector3 Result = Vector3.zero;

        if (impact.magnitude > 0.2) Result += impact * TickRate.GetMinTimeBetweenTicks();

        if (impact.magnitude > 0.2)
        {
            impact -= new Vector3(impact.x * airResistence, impact.y * airResistence, impact.z * airResistence) * TickRate.GetMinTimeBetweenTicks();
        }

        if (impact.magnitude < 0)
            impact = Vector3.zero;

        if (isGroundeed() && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
        }

        Result += moveDirection.normalized * TickRate.GetMinTimeBetweenTicks() * movementSpeed;

        if (input.inputs[0] && isGroundeed() && !GetDisableInput())
        {
            ResetPlayerVelocity();
            AddImpact(Vector3.up, jumpHeight, false);
            StartCoroutine(ChkIfJumped());
            //Callin the event for children classes
            onJump();
        }

        //Abilites

        if (input.inputs[3] && !GetDisableInput())
        {
            StartAbility(0);
        }

        if (input.inputs[1] && !GetDisableInput())
        {
            StartAbility(1);
        }

        //if (GetComponent<ServerAuthoritativeTransform>().clientInput.l_mouse && !GetDisableInput() && GetComponent<Health>().canInfluenceDamage)
        if (input.inputs[2] && !GetDisableInput())
        {
            StartAbility(2);
        }

        if (!characterController.isGrounded)
        {
            AddImpact(Vector3.up, gravity, false);
        }

        characterController.Move(Result + (playerVelocity * TickRate.GetMinTimeBetweenTicks()));

        return new MovementResult()
        {
            position = transform.position,
            tick = input.tick
        };
    }

    public void Update()
    {
        //CameraPosistionAdjustment

        piviot_M.transform.position = folllowTarget.position;

        //Camera Movement

        if (isLocalPlayer)
        {
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
    public float GetOriginalGraivty()
    {
        return originalGravity;
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

    public void DisableRotationInput(bool enabled)
    {
        disableRotationInput = enabled;
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

    public bool GetDisableRotationInput()
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

        if (!abilityRef.canCast || abilityInProgress || abilityRef.isDisabled) return;

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

            float timer = abilityRef.coolDown;

            if (!abilityRef.skipNextCoolDown)
            {
                int c_tick = TickRate.Instance.currentTick;

                for (float z = c_tick; z < c_tick + (abilityRef.coolDown * TickRate.SERVER_TICK_RATE);)
                {
                    z = TickRate.Instance.currentTick;
                    abilityRef.coolDown_current_value = (z - c_tick) / TickRate.SERVER_TICK_RATE;
                }
            }
            else
                abilityRef.skipNextCoolDown = true;

            abilityRef.canCast = true;

            yield return null;
        }
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

    public void ShakeCamera(float intensity)
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
            cineCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        StartCoroutine(timer());

        IEnumerator timer()
        {
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
            yield return new WaitForSeconds(intensity / 3);
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0;
        }
    }

    /*
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
    */

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

            rigidbody.AddForceAtPosition((forcedirection * 25 / rigidbody.mass) * TickRate.GetMinTimeBetweenTicks(), transform.position, ForceMode.Impulse);
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
                case BasePowerUp.effect.Speed: if (HasSpeedUp == false) { StartCoroutine(SpeedUp(OtherPowerup.EffectDuration)); } break;
                case BasePowerUp.effect.Jump: if (HasJumpUp == false) { StartCoroutine(JumpUp(OtherPowerup.EffectDuration)); } break;
                case BasePowerUp.effect.Cooldown: if (HasCoolUp == false) { StartCoroutine(CoolDownUp(OtherPowerup.EffectDuration)); } break;
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

        if (b) { result = 1.7f; if (!pointer) { pointer = (GameObject)Instantiate(Resources.Load("PointerCanvas")); } }
        else { result = 4.5f; Destroy(pointer); }

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

[Serializable]
public class Ability
{

    public Action ability;

    public float coolDown;
    public float coolDown_current_value = 0;

    public string abilityName = "NoName (please assign a name)";

    [HideInInspector] public UnityEvent[] events;
    [HideInInspector] public Action End;

    [HideInInspector] public bool canCast = true;
    [HideInInspector] public bool skipNextCoolDown = false;
     public bool isDisabled = false;

    public void DisableAbility(bool b)
    {
        isDisabled = b;
    }

}

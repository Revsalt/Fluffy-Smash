using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;

public class PlayerController : NetworkBehaviour
{
    [HideInInspector]public CharacterController characterController;
    public GameObject piviot_M;
    public Vector3 playerVelocity = Vector3.zero;
    [SerializeField] public Vector3 playerVelocityResult = Vector3.zero;
    Vector3 impact = Vector3.zero;
    bool disableMovement = false;
    bool restricMovment = false;

    [Header("Default")]
    [SerializeField] public GameObject playerModel;
    [SerializeField] public GameObject playerModelChild;
    [Header("Camera")]
    [SerializeField]private float sensitvity = 100;
    [Header("Movement")]
    public float movementSpeed = 5;
    [Header("Jumping")]
    [SerializeField]private float jumpHeight = 5;
    public LayerMask layerMask = 5;
    [SerializeField] public float gravity = 5;
    [Header("Other Passives and Ablities")]
    [SerializeField] float dashForce = 80;
    [SerializeField] float dashCooldown = 2;
    float currentDashCooldown = 0;
    [SerializeField] float wallJumpForce = 80;
    Vector3 wallDirection = Vector3.zero;

    ControllerColliderHit ColidedWithWall = null;

    [HideInInspector]
    public float velocity;

    void OnValidate()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        piviot_M.transform.SetParent(null);
    }

    float rotX, rotY;
    public void Update()
    {
        if (!isLocalPlayer) return;

        Vector3 move = transform.right * Input.GetAxisRaw("Horizontal") +
        transform.forward * Input.GetAxisRaw("Vertical");

        if (move != Vector3.zero && !GetDisableMovement())
        {
            transform.rotation = Quaternion.Euler(0, piviot_M.transform.eulerAngles.y, 0);
            playerModelChild.transform.forward = move;
        }

        //Camera Movement

        rotX += Input.GetAxis("Mouse X") * sensitvity * Time.deltaTime;
        rotY += Input.GetAxis("Mouse Y") * sensitvity * Time.deltaTime;

        rotY = Mathf.Clamp(rotY, -80f, 80f);

        piviot_M.transform.localRotation = Quaternion.Euler(-rotY, rotX, 0f);

    }

    private void LateUpdate()
    {
        //CameraPosistionAdjustment
        piviot_M.transform.position = transform.position;
    }

    bool harshfall = false;
    public StatePayload Movement(InputPayload input, float deltaTime)
    {
        velocity = playerVelocity.magnitude;

        Vector3 move = new Vector3(input.x , input.y , input.z);

        if (playerVelocity.y < -20)
        {
            Debug.Log("isHarshFall");
            harshfall = true;
        }

        if (Physics.Raycast(transform.position , Vector3.down , 3f) && harshfall)
        {
            Debug.Log("HarshFall");
            GetComponent<PlayerAnimations>().animator.Play("Roll");  
        }

        if (isGroundeed())
        {
            if (harshfall == true)
            {
                AddImpact(move, 50);
            }

            harshfall = false;
        }

        if (wallDirection == Vector3.zero && move != Vector3.zero)
            playerModelChild.transform.forward = move;

        if (wallDirection != Vector3.zero && (input.bools & 1) != 0)
        {
            DisableMovment(false);
            GetComponent<PlayerAnimations>().animator.SetTrigger("EndWallGrab");

            transform.rotation = Quaternion.Euler(Vector3.zero);

            if (Physics.Raycast(transform.position, -wallDirection, 10, layerMask))
            {
                if (move != Vector3.zero)
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

        //conditional forces here

        if ((input.bools & 2) != 0 && !disableMovement && currentDashCooldown <= 0)
        {
            currentDashCooldown = dashCooldown;
            Vector3 direc = move.normalized;
            direc.y = 0;

            if (move != Vector3.zero) 
                AddImpact(direc, dashForce);
            else
                AddImpact(Vector3.up, dashForce);

            GetComponent<PlayerAnimations>().animator.Play("Dash");
        }

        if (currentDashCooldown > 0)
        {
            currentDashCooldown -= 1 * deltaTime;
        }

        //Movement and impact
        if (!disableMovement)
        {
            Vector3 Result = Vector3.zero;

            Result += move * movementSpeed;

            if (impact.magnitude > 0.2) Result += impact;
            impact = Vector3.Lerp(impact, Vector3.zero, 5 * deltaTime);

            if (characterController.isGrounded && playerVelocity.y < 0)
            {
                playerVelocity.y = 0;
            }

            if ((input.bools & 1) != 0 && isGroundeed())
            {
                playerVelocity.y = 0;
                playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);
            }

            //apply the motion

            playerVelocity.y += gravity;


            characterController.Move((Result + playerVelocity) * deltaTime);
            playerVelocityResult = ((Result + playerVelocity) * deltaTime);
        }

        if (ColidedWithWall != null)
        {
            GrabWall(ColidedWithWall);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, 2);

        foreach (var item in colliders)
        {
            if (item.GetComponent<PlayerController>() && !item.GetComponent<NetworkIdentity>().isLocalPlayer)
            {
                Debug.Log("stunPlayer");
            }
        }

        return new StatePayload()
        {
            tick = input.tick,
            position = transform.position,
        };
    }

    void ResetTrasnformValues(Transform trans)
    {
        trans.transform.localPosition = Vector3.zero;
        trans.transform.localEulerAngles = Vector3.zero;
    }

    public void SetPlayerPosition(Vector3 newPosition)
    {
        characterController.enabled = false;
        transform.position = newPosition;
        characterController.enabled = true;
    }

    public void RestrictMovement(bool b)
    {
        restricMovment = b;
    }

    public void DisableMovment(bool enabled)
    {
        disableMovement = enabled;

        characterController.Move(Vector3.zero);

        if (enabled)
            impact = Vector3.zero;
    }

    public bool GetDisableMovement()
    {
        return disableMovement;
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
        characterController.velocity.Set(0, 0, 0);
    }

    public bool isGroundeed()
    {
        return Physics.CheckSphere(transform.position - new Vector3(0, characterController.height / 2, 0), .4f, layerMask);
    }

    private void OnDrawGizmos()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, -Vector3.up, out hit, LayerMask.GetMask("Player"), 99999);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, hit.point);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (wallDirection != Vector3.zero)
            return;

        if ((characterController.collisionFlags & CollisionFlags.Sides) != 0 && DistanceBetweenGround() > 1.5f && WallHeightIsEnough(hit.normal))
        {
            ColidedWithWall = hit;
        }
    }

    public void GrabWall(ControllerColliderHit hit)
    {
        ResetPlayerVelocity();
        DisableMovment(true);
        transform.position += hit.normal / 2;

        wallDirection = hit.normal;
        transform.rotation = Quaternion.LookRotation(Vector3.up, wallDirection);

        GetComponent<PlayerAnimations>().animator.Play("WallGrab");

        ColidedWithWall = null;

    }

    private bool WallHeightIsEnough(Vector3 wallDirection)
    {
        bool condition = false;

        for (int i = 0; i < 2; i++)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position + new Vector3(0, i * .5f, 0), -wallDirection, out hit, 1);
            Debug.DrawRay(transform.position + new Vector3(0, i * .5f, 0), -wallDirection, Color.blue, 5);

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

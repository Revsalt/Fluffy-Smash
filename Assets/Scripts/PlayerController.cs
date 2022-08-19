using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    [HideInInspector]public CharacterController characterController;
    public GameObject piviot_M;
    Vector3 playerVelocity = Vector3.zero;
    Vector3 impact = Vector3.zero;
    bool disableMovement = false;

    [Header("Default")]
    [SerializeField] public GameObject playerModel;
    [Header("Camera")]
    [SerializeField]private float sensitvity = 100;
    [Header("Movement")]
    public float movementSpeed = 5;
    [SerializeField] private float slopeForce;
    [SerializeField] private float slopeForceRayLength;
    [Header("Jumping")]
    [SerializeField]private float jumpHeight = 5;
    public LayerMask layerMask = 5;
    [SerializeField] private float gravity = 5;


    public GameObject[] Cameras;

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
        if (!isLocalPlayer)
            return;

        foreach (var item in Cameras)
        {
            item.SetActive(true);
        }
        
        Vector3 move = transform.right * Input.GetAxisRaw("Horizontal") +
            transform.forward * Input.GetAxisRaw("Vertical");

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

        if (move != Vector3.zero)
            transform.rotation = Quaternion.Euler(0,piviot_M.transform.eulerAngles.y , 0);

        //Movement and impact

        Vector3 Result = Vector3.zero;

        if (impact.magnitude > 0.2) Result += impact * Time.deltaTime;
         impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);
        

        if (characterController.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
        }

        Result += move * Time.deltaTime * movementSpeed;

        if (Input.GetKeyDown(KeyCode.Space) && isGroundeed())
        {
            playerVelocity.y = 0;
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }

        playerVelocity.y += gravity * Time.deltaTime * 3;
        characterController.Move(Result + (playerVelocity * Time.deltaTime));

        if ((move != Vector3.zero) && OnSlope())
            characterController.Move(Vector3.down * characterController.height / 2 * slopeForce * Time.deltaTime);

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

    private bool OnSlope()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, characterController.height / 2 * slopeForceRayLength))
            if (hit.normal != Vector3.up)
                return true;
        return false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Mirror;

public class PlayerAnimations : NetworkBehaviour
{
    public Animator animator;
    public Transform playerModelIkTarget;
    public CinemachineVirtualCamera virtualCamera;
    public CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] float FOVmultiplier = .2f;

    private PlayerController pc;

    private void Start()
    {
        pc = GetComponent<PlayerController>();
    }

    float timeinAir = 0;
    [ClientCallback]
    void Update()
    {
        float newFOV = GetComponent<PlayerController>().playerVelocityResult.magnitude * FOVmultiplier;
        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView , 50 + newFOV, 5 * Time.deltaTime);

        Vector3 movementDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        bool isRunning = movementDirection != Vector3.zero;
        if (isLocalPlayer)
        {
            animator.SetBool("isRun", isRunning);
            animator.SetBool("isJump", !pc.isGroundeed());
            animator.SetFloat("runSpeed", pc.movementSpeed / 7);
            animator.SetFloat("YVelo", GetComponent<PlayerController>().playerVelocityResult.y);

            if (!pc.isGroundeed())
            {
                timeinAir -= Time.deltaTime;
                if (timeinAir < .1f)
                {
                    
                }
                animator.SetFloat("timeInAir", timeinAir);

            }
            else
            {
                timeinAir = 1.2f;
            }
        }
    }

    [ClientCallback]
    private void LateUpdate()
    {
        Vector3 pos = transform.position + virtualCamera.transform.forward * 10 + Vector3.up;
        playerModelIkTarget.transform.position = pos;
    }
}

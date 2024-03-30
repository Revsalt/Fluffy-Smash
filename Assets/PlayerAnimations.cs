using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerAnimations : MonoBehaviour
{
    public Animator animator;
    public CinemachineVirtualCamera virtualCamera;
    public CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] float FOVmultiplier = .2f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float newFOV = GetComponent<Player>().velocity * FOVmultiplier;
        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView , 50 + newFOV, 5 * Time.deltaTime);

        animator.SetFloat("YVelo" , GetComponent<CharacterController>().velocity.y);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DefaultCharacterEffects : MonoBehaviour
{
    [Header("ParticleSystems")]
    ParticleSystem movementParticleSystem;
    ParticleSystem runAirEffect;
    [Header("hitGround")]
    [SerializeField] UnityEvent HitGroundNormal;
    [SerializeField] UnityEvent HitGroundHarsh;
    [Header("inAir")]
    [SerializeField] UnityEvent inAirStart;
    [SerializeField] UnityEvent inAirEnd;
    [Header("Other")]
    [SerializeField] Camera m_Camera;

    PlayerController playerController;
    Animator animator;
    bool ranHarshFallFunction = false;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        animator = playerController.animator;

        movementParticleSystem = ((GameObject)Instantiate(Resources.Load<GameObject>("DefaultEffects/MovementParticleSystem"),
            transform.position - Vector3.up, Quaternion.identity , transform)).GetComponent<ParticleSystem>();

        runAirEffect = ((GameObject)Instantiate(Resources.Load<GameObject>("DefaultEffects/CameraAnimeEffect"),
            m_Camera.transform.position + m_Camera.transform.forward * 3f, Quaternion.identity 
            , m_Camera.transform)).GetComponent<ParticleSystem>();
    }

    void Update()
    {
        //ParticleSystem
        var mps = movementParticleSystem.main;
        mps.simulationSpeed = Mathf.Lerp(animator.GetFloat("runSpeed"), playerController.characterController.velocity.sqrMagnitude * 1.5f, 5 * Time.deltaTime);

        ParticlesSystemEnabled(movementParticleSystem, playerController.isGroundeed());
        ParticlesSystemEnabled(runAirEffect, playerController.characterController.velocity.magnitude > 0.1f);

        var t = runAirEffect.main;
        t.simulationSpeed = playerController.movementSpeed * 0.4f;

        if (!playerController.isGroundeed())
        {
            inAirStart.Invoke();

            if (!ranHarshFallFunction)
                StartCoroutine(HarshFall());

            IEnumerator HarshFall()
            {
                bool harshFall = false;
                ranHarshFallFunction = true;

                for (float i = 0; !playerController.isGroundeed(); i += Time.deltaTime)
                {
                    if (i > 1)
                        harshFall = true;
                    yield return null;
                }

                inAirEnd.Invoke();

                ParticleSystem p = ((GameObject)Instantiate(Resources.Load<GameObject>("DefaultEffects/Land") , transform.position - Vector3.up, Quaternion.identity)).GetComponent<ParticleSystem>();
                p.Play();
                Destroy(p.gameObject, 1);

                HitGroundNormal.Invoke();
                playerController.ShakeCamera(1, .2f);
                if (harshFall)
                {
                    HitGroundHarsh.Invoke();
                }

                ranHarshFallFunction = false;
            }
        }

    }

    public static void ParticlesSystemEnabled(ParticleSystem ps, bool b)
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
}

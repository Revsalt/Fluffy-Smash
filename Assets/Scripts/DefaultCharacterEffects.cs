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
    [SerializeField]float windVolumeMultiplaier = .5f;
    public Camera m_Camera;

    PlayerController playerController;
    Animator animator;
    bool ranHarshFallFunction = false;
    float DefaultFOV;

    AudioSource windAUD;

    private void Start()
    {
        windAUD = AudioManager.instance.Play2DAUD("wind");
        playerController = GetComponent<PlayerController>();
        animator = playerController.animator;

        movementParticleSystem = ((GameObject)Instantiate(Resources.Load<GameObject>("DefaultEffects/MovementParticleSystem"),
            transform.position - Vector3.up, Quaternion.identity , transform)).GetComponent<ParticleSystem>();

        runAirEffect = ((GameObject)Instantiate(Resources.Load<GameObject>("DefaultEffects/CameraAnimeEffect"),
            m_Camera.transform.position + m_Camera.transform.forward, Quaternion.identity
            , m_Camera.transform)).GetComponent<ParticleSystem>();

        DefaultFOV = playerController.cineCamera.m_Lens.FieldOfView;
    }

    void Update()
    {
        
        windAUD.volume = Mathf.Lerp(windAUD.volume , GetComponent<CharacterController>().velocity.magnitude * windVolumeMultiplaier , 5 * Time.deltaTime);
        //ParticleSystem
        playerController.cineCamera.m_Lens.FieldOfView = Mathf.Lerp(playerController.cineCamera.m_Lens.FieldOfView , DefaultFOV + GetCharacterMagintude(playerController , .7f) , 7 * Time.deltaTime);

        ParticlesSystemEnabled(movementParticleSystem, playerController.isGroundeed());
        ParticlesSystemEnabled(runAirEffect, GetCharacterMagintude(playerController , 1) > 10f);

        var t = runAirEffect.main;
        t.simulationSpeed = playerController.movementSpeed * 0.1f;

        var z = movementParticleSystem.main;
        z.simulationSpeed = playerController.movementSpeed * 0.1f;

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

                ParticleSystem p = ((GameObject)Instantiate(Resources.Load<GameObject>("DefaultEffects/Land") , transform.position - Vector3.up, Quaternion.Euler(90,0,0))).GetComponent<ParticleSystem>();
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

    public static float GetCharacterMagintude(PlayerController pc , float multiplier)
    {
        if (!pc.GetDisableMovement())
            return pc.characterController.velocity.magnitude * multiplier;
        else
            return 0;

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using Mirror;

public class HoodieCat : PlayerController
{
    [SerializeField] UnityEvent StartGroundSmash, EndGroundSmash;
    [SerializeField] UnityEvent StartTeleport, EndTeleport;
    [SerializeField] CinemachineVirtualCamera zoomCamera;
    [SerializeField] Transform pointerCastPosition;
    [SerializeField] GameObject pointer,LoliPop;
    [SerializeField] GameObject[] modelHidden,modelNormal;

    Animator animator;
    float oldGravity;
    private void Start()
    {
        animator = playerModel.GetComponentInChildren<Animator>();

        oldGravity = gravity;

        onJump += delegate { Debug.Log("Jump"); };

        ability0 = new Ability()
        {
            ability = delegate
            {
                StartCoroutine(AttackSquence0());
            },
            coolDown = 5f,
            events = new UnityEvent[2] { StartGroundSmash, EndGroundSmash }
        };

        ability1 = new Ability()
        {
            ability = delegate
            {
                StartCoroutine(AttackSequence1());
            },
            coolDown = 1f,
            events = new UnityEvent[2] { StartTeleport, EndTeleport }
        };
    }

    private void Update()
    {
        base.Update();

        animator.SetBool("isMove", moveDirection != Vector3.zero);
        animator.SetFloat("runSpeed", movementSpeed / 7);

        playerModel.transform.LookAt(playerModel.transform.position + moveDirection);

        if (Camera.main)
        {
            Vector3 pos = playerModel.transform.position + Camera.main.transform.forward * 10;
            //playerModelIkTarget.transform.position = new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
        }

        animator.SetBool("isJump", !isGroundeed());
    }

    public void ZoomCamera(bool b)
    {
        if (b)
            zoomCamera.Priority = 2;
        else
            zoomCamera.Priority = 0;

        pointer.SetActive(b);
    }

    IEnumerator AttackSquence0()
    {
        ResetPlayerVelocity();
        DisableInput(true);
        AddImpact(Vector3.up, 100 , true);
        gravity = 0.1f;
        ZoomCamera(true);


        for (float i = 0; i < 2; i += Time.deltaTime)
        {
            if (Input.GetMouseButton(0))
                break;

            yield return null;
        }

        ZoomCamera(false);
        gravity = oldGravity;
        AddImpact(pointerCastPosition.transform.forward, 500 , false);

        for (float i = 0; !isGroundeed(); i += Time.deltaTime)
        {
            yield return null;
        }

        ShakeCamera(4, 0.2f);
        DisableMovment(true);

        yield return new WaitForSeconds(.3f);

        DisableMovment(false);
        DisableInput(false);

        ability0.End.Invoke();

        yield return null;
    }

    IEnumerator AttackSequence1()
    {
        TransclucentMode(true);
        GameObject CrossPointer = Instantiate(Resources.Load("RoundMarker") as GameObject, new Vector3(0, 0, 0), Quaternion.identity, null);

        RaycastHit hit = new RaycastHit();
        Vector3 lastHitNormal = Vector3.zero;

        for (float i = 0; !Input.GetKeyUp(KeyCode.LeftShift); i += Time.deltaTime)
        {
            Physics.Raycast(pointerCastPosition.position, pointerCastPosition.transform.forward, out hit, 20, layerMask);
            if (hit.collider)
            {
                if (hit.normal.y * 90f >= -20 && hit.normal.y * 90f == 0)
                {
                    CrossPointer.transform.position = hit.point;
                    CrossPointer.transform.rotation = Quaternion.LookRotation(pointerCastPosition.transform.forward);

                    CrossPointer.SetActive(true);
                    lastHitNormal = hit.normal;
                }
            }
            else { CrossPointer.SetActive(false); }
            yield return null;

        }

        if (lastHitNormal != Vector3.zero && CrossPointer.activeSelf)
        {
            LoliPop.transform.SetParent(null);
            for (float i = 0; Vector3.Distance(LoliPop.transform.position, CrossPointer.transform.position) > .1f; i += Time.deltaTime)
            {
                LoliPop.transform.position = Vector3.Lerp(LoliPop.transform.position, CrossPointer.transform.position, 10 * Time.deltaTime);
                LoliPop.transform.rotation = Quaternion.LookRotation(lastHitNormal);
                yield return null;
            }
            LoliPop.GetComponent<BoxCollider>().enabled = true;

            SetPlayerPosition(CrossPointer.transform.position + lastHitNormal + Vector3.up);
        }

        TransclucentMode(false);
        Destroy(CrossPointer);

        ability1.End.Invoke();

        yield return null;
    }

    void TransclucentMode(bool b)
    {
        foreach (var item in modelHidden)
        {
            item.SetActive(b);
        }

        foreach (var item in modelNormal)
        {
            item.SetActive(!b);
        }
    }
}

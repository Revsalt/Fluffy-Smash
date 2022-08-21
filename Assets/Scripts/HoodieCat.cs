using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using Mirror;

public class HoodieCat : PlayerController
{
    [SerializeField] UnityEvent StartGroundSmash, EndGroundSmash;
    [SerializeField] CinemachineVirtualCamera zoomCamera;
    [SerializeField] Transform pointerCastPosition;
    [SerializeField] GameObject pointer;

    float oldGravity;
    private void Start()
    {
        oldGravity = gravity;

        onJump += delegate { Debug.Log("Jump"); };

        ability0 = new Ability()
        {
            ability = delegate
            {
                StartCoroutine(AttackSquence());

                IEnumerator AttackSquence()
                {
                    DisableInput(true);
                    AddImpact(Vector3.up, 100);
                    gravity = 0.1f;
                    ZoomCamera(true);

                    GameObject RoundPointer = Instantiate(Resources.Load("RoundMarker") as GameObject, new Vector3(0,0,0), Quaternion.identity, null);

                    for (float i = 0; i < 2; i += Time.deltaTime)
                    {
                        Physics.Raycast(pointerCastPosition.position , pointerCastPosition.transform.forward, out RaycastHit hit , 50 , layerMask);
                        if (hit.collider)
                        {
                            if (hit.normal.y * 90f != 0)
                            {
                                RoundPointer.transform.position = hit.point;
                                RoundPointer.transform.rotation = Quaternion.LookRotation(pointerCastPosition.transform.forward);
                            }
                        }

                        if (Input.GetMouseButton(0))
                            break;

                        yield return null;
                    }

                    ZoomCamera(false);
                    gravity = oldGravity;
                    AddImpact((transform.position - RoundPointer.transform.position).normalized, -600);
                    Destroy(RoundPointer);

                    for (float i = 0; !isGroundeed(); i += Time.deltaTime)
                    {
                        yield return null;
                    }

                    DisableMovment(true);

                    yield return new WaitForSeconds(1);

                    DisableMovment(false);
                    DisableInput(false);

                    yield return null;
                }
            },
            coolDown = 5f,
            events = new UnityEvent[2] { StartGroundSmash, EndGroundSmash }
        };

    }

    private void Update()
    {
        base.Update();

    }

    public void ZoomCamera(bool b)
    {
        if (b)
            zoomCamera.Priority = 2;
        else
            zoomCamera.Priority = 0;

        pointer.SetActive(b);
    }
}

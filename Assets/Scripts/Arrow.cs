using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] float multiplier = 1;
    [SerializeField] bool isUp = false;

    [SerializeField] LayerMask layers;
    bool pressed = false;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 200 , layers))
            {
                if (hit.transform.gameObject == this.gameObject)
                    pressed = true;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            pressed = false;
        }

        if (pressed)
        {
            string inputName = "Mouse X";
            int sign = 1;

            if (isUp)
            {
                inputName = "Mouse Y";
            }
            else
            {
                if (isMyDirection())
                {
                    inputName = "Mouse Y";
                    if (isCorrectDirectionUp())
                        sign = -1;
                    else
                        sign = 1;
                }
                else
                {
                    inputName = "Mouse X";
                    if (isCorrectDirectionRight())
                        sign = -1;
                    else
                        sign = 1;
                }
            }

            transform.parent.transform.parent.transform.position += (Input.GetAxisRaw(inputName) * sign) * transform.forward * Time.deltaTime * multiplier;
        }
    }

    bool isMyDirection()
    {
        Vector3 dirc = Camera.main.transform.forward;
        Vector3 dircRounded = new Vector3(Mathf.Round(dirc.x), 0, Mathf.Round(dirc.z));
        if (dircRounded == transform.forward || dircRounded == -transform.forward)
        {
            Debug.Log(dircRounded + " : " + transform.forward + " :: mydirection");
            return true;
        }
        else
        {
            Debug.Log(dircRounded + " : " + transform.forward + " :: mydirection");
            return false;
        }
    }

    bool isCorrectDirectionUp()
    {
        Vector3 dirc = Camera.main.transform.forward;
        Vector3 dircRounded = new Vector3(Mathf.Round(dirc.x), 0, Mathf.Round(dirc.z));
        if (dircRounded == -transform.forward)
        {
            Debug.Log(dircRounded + " : " + -transform.forward + " :: correctdirectionUp");
            return true;
        }
        else
        {
            Debug.Log(dircRounded + " : " + -transform.forward + " :: correctdirectionUp");
            return false;
        }
    }

    bool isCorrectDirectionRight()
    {
        Vector3 dirc = Camera.main.transform.forward;
        Vector3 dircRounded = new Vector3(Mathf.Round(dirc.x), 0, Mathf.Round(dirc.z));
        if (dircRounded == transform.right)
        {
            Debug.Log(dircRounded + " : " + transform.right + " :: correctdirectionRight");
            return true;
        }
        else
        {
            Debug.Log(dircRounded + " : " + transform.right + " :: correctdirectionRight");
            return false;
        }
    }
}

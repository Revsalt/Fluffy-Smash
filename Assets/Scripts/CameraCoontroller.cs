using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraCoontroller : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float sensitivity = 100;
    [Header("UI")]
    [SerializeField] Text speedDisplay;
    [SerializeField] Text selectedObject;

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            transform.position += transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * speed;
            transform.position += transform.right * Input.GetAxis("Horizontal") * Time.deltaTime * speed;

            transform.eulerAngles += new Vector3(-Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity, Input.GetAxis("Mouse X") * Time.deltaTime * sensitivity, 0);

            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                speed = Mathf.Clamp((speed + 1), 0, 10);
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                speed = Mathf.Clamp((speed - 1), 0, 10);
            }

            speedDisplay.text = speed.ToString();
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}

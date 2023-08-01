using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class CursorPosition : MonoBehaviour
{
    public float depth;
    public Vector3 Offset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var screenPoint = Input.mousePosition;
        screenPoint.z = depth; //distance of the plane from the camera
        transform.position = Camera.main.ScreenToWorldPoint(screenPoint) + Offset;
    }
}

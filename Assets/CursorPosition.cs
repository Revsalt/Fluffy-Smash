using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
        //var screenPoint = Mouse.current.position.ReadValue();
        //transform.position = Camera.main.ScreenToWorldPoint(screenPoint) + Offset;
    }
}

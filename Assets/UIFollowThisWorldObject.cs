using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollowThisWorldObject : MonoBehaviour
{
    public Transform theObject;

    void Update()
    {
        float dist = Vector3.Distance(Camera.main.transform.position, theObject.transform.position);
        float distClamped = (1 / dist) * 10;
        transform.localScale = new Vector3(distClamped, distClamped, distClamped);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(theObject.transform.position);
        if (screenPos.z > 0)
            transform.position = new Vector3(Mathf.Clamp(screenPos.x , 0 , Screen.width), Mathf.Clamp(screenPos.y, 0, Screen.height), 0);
    }
}

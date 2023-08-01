using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationOtherEvent : MonoBehaviour
{   
    public UnityEvent[] uis;
    public void IvokeEvent(int eventId)
    {
        uis[eventId].Invoke();
    }
}

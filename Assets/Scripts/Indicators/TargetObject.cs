using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetObject : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(KeepLoopinTillitsHere());
    }

    IEnumerator KeepLoopinTillitsHere()
    {
        UIController ui = GetComponentInParent<UIController>();

        for (float i = 0; ui == null; i += Time.deltaTime)
        {
            ui = FindObjectOfType<UIController>();
            yield return null;
        }

        ui.AddTargetIndicator(this.gameObject);
    }
}

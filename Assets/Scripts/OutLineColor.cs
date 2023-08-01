using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutLineColor : MonoBehaviour
{
    public void SetColor(Color c)
    {
        foreach (var item in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            foreach (var _item in item.GetComponent<Renderer>().materials)
            {
                _item.SetColor("_OutlineColor" , c);
            }
            
        }
    }
}

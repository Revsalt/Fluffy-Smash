using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Canvas canvas;

    public Camera cam;

    public List<TargetIndicator> targetIndicators = new List<TargetIndicator>();

    public GameObject TargetIndicatorPrefab;

    // Update is called once per frame
    void Update()
    {
        if (targetIndicators.Count > 0)
        {
            for(int i = 0; i < targetIndicators.Count; i++)
            {
                targetIndicators[i].UpdateTargetIndicator();
            }
        }
    }

    public void AddTargetIndicator(GameObject target)
    {
        TargetIndicator indicator = GameObject.Instantiate(TargetIndicatorPrefab, canvas.transform).GetComponent<TargetIndicator>();
        indicator.InitialiseTargetIndicator(target, cam, canvas);
        targetIndicators.Add(indicator);
    }

}

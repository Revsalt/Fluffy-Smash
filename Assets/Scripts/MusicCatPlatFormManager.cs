using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicCatPlatFormManager : MonoBehaviour
{
    [SerializeField] GameObject[] Clouds;
    [SerializeField] GameObject stringModel;

    public void UpdateCloudPlatform(float Distance)
    {
        Clouds[0].transform.localPosition = new Vector3(0, 0, Distance);
        Clouds[1].transform.localPosition = new Vector3(0, 0, -Distance);
        stringModel.transform.localScale = new Vector3(1, 1, Distance);
    }

}

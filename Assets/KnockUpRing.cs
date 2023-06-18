using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class KnockUpRing : NetworkBehaviour
{
    [SerializeField] AnimationCurve sizeOverTime;
    [SerializeField] Vector3 size3D;
    [SyncVar]public float sizeMultiplier = 1;

    public NetworkIdentity knockUpCaster;

    Material currentMat;

    void Start()
    {
        currentMat = gameObject.GetComponent<Renderer>().material;
        StartCoroutine(SizeOverTime());
    }

    IEnumerator SizeOverTime()
    {
        for (float i = 0; i < sizeOverTime.length; i += Time.deltaTime)
        {
            transform.localScale = new Vector3(size3D.x * sizeOverTime.Evaluate(i) * sizeMultiplier, size3D.y * sizeOverTime.Evaluate(i) * sizeMultiplier, size3D.z * sizeOverTime.Evaluate(i) * sizeMultiplier);
            yield return null;
        }
    }

    void Update()
    {
        Color oldColor = currentMat.color;
        ChangeAlpha(oldColor.a - .4f * Time.deltaTime);
    }

    void ChangeAlpha(float alphaVal)
    {
        Color oldColor = currentMat.color;
        Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, alphaVal);
        currentMat.SetColor("_BaseColor", newColor);

    }


    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Health>() == null) return;

        if (other.GetComponent<NetworkIdentity>() != knockUpCaster)
        {
            other.GetComponent<Health>().Kill(other.GetComponent<NetworkIdentity>() , knockUpCaster);
        }
    }

}

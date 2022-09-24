using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WhiteCat : PlayerController
{
    [SerializeField] Renderer myrenderer;
    Color color;
    private void Start()
    {
        //color = myrenderer.material.color;
        //ability0 = new Ability
        //{
        //    ability = delegate
        //    {
        //        cmdSendInvisStatus(GetComponent<NetworkIdentity>(),ability0.coolDown);
        //        SetTranslucent(ability0.coolDown);
        //        ability0.End.Invoke();
        //    }
        //    ,
        //    coolDown = 5f
        //    ,
        //    events = new UnityEvent[2] { new UnityEvent(), new UnityEvent() }
        //};
    }
    private void Update()
    {
        
    }
    IEnumerator SetTranslucent(float Duration)
    {
        LerpAlpha(0,false);
        yield return new WaitForSeconds(Duration);
        LerpAlpha(1,false);
    }
    IEnumerator LerpAlpha(float endvalue,bool IsTranparent)
    {
        float startvalue = color.a;
        for (float i = 0; i < 2f; i += Time.deltaTime)
        {
            color.a = Mathf.Lerp(startvalue, endvalue, 5 * Time.deltaTime);
            if (IsTranparent && color.a == endvalue)
            {
                playerModel.SetActive(!playerModel.activeSelf);
            }
            yield return null;
        }
    }
    IEnumerator SetInvisibillity(float Duration)
    {
        LerpAlpha(0,true);
        yield return new WaitForSeconds(Duration);
        LerpAlpha(1,true);
    }
    [Command]
    void cmdSendInvisStatus(NetworkIdentity ntd,float duration)
    {
        RpcSetInvis(ntd,duration);
    }
    [ClientRpc]
    void RpcSetInvis(NetworkIdentity ntd,float duration)
    {
        if (ntd.isLocalPlayer)
        {
            return;
        }
        SetInvisibillity(duration);
    }
}

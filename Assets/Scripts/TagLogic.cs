using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class TagLogic : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnChangeTaggerState))] public bool isTagger = false;
    public bool CanTag = false;
    public Transform TagPosition;
    public float TagRadius = 2;

    public UnityEvent OnTagged;

    public delegate void isTagging(bool b);
    public event isTagging onTag;

    bool canGrab = true;

    public void OnChangeTaggerState(bool oldb , bool newb)
    {
        if (newb)
            SetTextColor(Color.red);
    }

    void SetTextColor(Color c)
    {
        GetComponent<PlayerNetworkManager>().usernametxt.color = c;
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (CanTag)
        {
            Collider[] players = Physics.OverlapSphere(TagPosition.position, TagRadius);
            foreach (var item in players)
            {
                if (item.GetComponent<TagLogic>() && !item.GetComponent<NetworkIdentity>().isLocalPlayer && !item.GetComponent<TagLogic>().isTagger)
                {
                    CmdIsTagged(item.gameObject);
                    CanTag = false;
                    break;
                }  
            }
        }

        PlayerController myPlayer = GetComponent<PlayerController>();

        if (Input.GetMouseButton(0) && !myPlayer.GetDisableMovement() && canGrab && isTagger)
        {

            StartCoroutine(PersonGrab());

            IEnumerator PersonGrab()
            {
                canGrab = false;
                myPlayer.movementSpeed = myPlayer.GetOriginalSpeeed() * 1.3f;
                onTag.Invoke(true);
                CanTag = true;

                for (float i = 0; i < 2; i += Time.deltaTime) //need to be tested
                {
                    if (CanTag == false)
                        break;
                    yield return null;
                }

                myPlayer.movementSpeed = myPlayer.GetOriginalSpeeed() * 2;
                onTag.Invoke(false);
                CanTag = false;

                yield return new WaitForSeconds(.8f);
                canGrab = true;
            }
        }
    }

    [Command]
    public void CmdIsTagged(GameObject item)
    {
        SendAnimationToAll(item);
        item.GetComponent<TagLogic>().isTagger = true;
        MakeTagger(item.GetComponent<NetworkIdentity>().connectionToClient);
    }

    [ClientRpc]
    public void SendAnimationToAll(GameObject item)
    {
        item.GetComponent<TagLogic>().OnTagged.Invoke();
    }

    [TargetRpc]
    public void MakeTagger(NetworkConnection ntd)
    {
        StartCoroutine(Tag());

        IEnumerator Tag()
        {
            ntd.identity.GetComponent<PlayerController>().enabled = false;
            yield return new WaitForSeconds(2);
            ntd.identity.GetComponent<PlayerController>().enabled = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(TagPosition.position, TagRadius);
    }
}

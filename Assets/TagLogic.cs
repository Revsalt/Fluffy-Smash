using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TagLogic : NetworkBehaviour
{
    [SyncVar]public string Role = "";

    private void Update()
    {
        if (!isLocalPlayer) return;

        if (Role == "Balls")
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                GetComponent<PlayerAnimations>().animator.SetBool("isPersonGrab", true);

                SendGrab();
            }
        }
    }

    public void CloseGrabAnimation()
    {
        GetComponent<PlayerAnimations>().animator.SetBool("isPersonGrab", false);
    }

    bool canGrab = true;
    [Command(requiresAuthority = false)]
    public void SendGrab(NetworkConnectionToClient sender = null)
    {
        if (!canGrab) return;

        StartCoroutine(Grab());

        IEnumerator Grab()
        {
            for (float i = 0; i < 2; i += Time.deltaTime)
            {
                Collider[] colliders = Physics.OverlapSphere(sender.identity.transform.position + sender.identity.transform.forward, 2);
                foreach (Collider collider in colliders)
                {
                    if (collider.GetComponent<TagLogic>() && collider.GetComponent<TagLogic>().Role != "Balls")
                    {
                        collider.GetComponent<TagLogic>().Role = "Balls";
                        GotGrabed(sender.identity.connectionToClient);
                        yield break;
                    }
                }
                yield return null;
            }

            GotGrabed(sender.identity.connectionToClient);

        }
    }

    [TargetRpc]
    public void GotGrabed(NetworkConnection target)
    {
        target.identity.GetComponent<TagLogic>().CloseGrabAnimation();
    }
}

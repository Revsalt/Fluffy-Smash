using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagDollPiece : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        AudioManager.instance.Play("BumpPieceSound" , transform.position , null);
    }

}

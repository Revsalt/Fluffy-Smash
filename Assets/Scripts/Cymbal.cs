using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Cymbal : NetworkBehaviour
{
    public float speed = 3;
    public float turnSpeed = 2;

    [HideInInspector]public Transform myCam;
    [HideInInspector]public Transform player;

    Vector3 startRotation;
    bool comeBack = false;
    bool disable = false;

    private void Start()
    {
        startRotation = myCam.eulerAngles;
    }

    float forw = 1;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) ComeBack(((myCam.transform.position + myCam.transform.forward * forw) - transform.position).normalized);

        if (comeBack || disable) return;

        forw += Time.deltaTime * speed;
        Vector3 move = myCam.transform.position + myCam.transform.forward * forw;

        transform.position = Vector3.MoveTowards(transform.position , move , Time.deltaTime * turnSpeed);
        transform.LookAt(move + Vector3.down * 2);
    }

    Vector3 stpos;
    void ComeBack(Vector3 curvdirec)
    {
        if (disable) return;

        comeBack = true;

        stpos = transform.position;

        StartCoroutine(cb());

        IEnumerator cb()
        {
            for (float i = 0; i < 1; i += Time.deltaTime)
            {
                transform.position = Bezier2(stpos , ((stpos + player.transform.position) / 2) + curvdirec * 5 , player.transform.position, i);
                transform.LookAt(player.transform.position + Vector3.down * 2);
                yield return null;
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerNetworkManager>()) return;

        StartCoroutine(delay());

        IEnumerator delay()
        {
            disable = true;
            yield return new WaitForSeconds(2);
            disable = false;

            ComeBack(myCam.transform.right);
        }
    }

    public static Vector3 Bezier2(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        var clamped_t = Mathf.Clamp(t, 0, 1);

        float u = 1 - clamped_t;
        float tt = clamped_t * clamped_t;
        float uu = u * u;
        Vector3 p = uu * p0;
        p += 2 * u * clamped_t * p1;
        p += tt * p2;

        return p;
    }
}

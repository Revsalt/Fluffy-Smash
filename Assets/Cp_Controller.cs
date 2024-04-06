using Mirror;
using System.Collections;
using System.Collections.Generic;
using Telepathy;
using UnityEngine;
using UnityEngine.UIElements;

public struct PlayerInput_CP
{
    public double timestamp;
    public Vector3 direction;

    public PlayerInput_CP(double timestamp, Vector3 direction)
    {
        this.timestamp = timestamp;
        this.direction = direction;
    }
}

public class Cp_Controller : NetworkBehaviour
{
    public Transform server;
    public Transform clientthen;

    public int positionHistorySize = 64;
    [SerializeField] SortedList<double, Vector3> position = new SortedList<double, Vector3>();

    [SerializeField] float speed;
    CharacterController cc;

    private void Start()
    {
        server.SetParent(null);
        clientthen.SetParent(null);
        cc = GetComponent<CharacterController>();
    }

    void Movement(PlayerInput_CP input_CP)
    {
        cc.Move(input_CP.direction * speed);
    }

    [ClientCallback]
    private void FixedUpdate()
    {
        PlayerInput_CP picp = new PlayerInput_CP(NetworkTime.time, new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")));

        if (position.Count >= positionHistorySize) position.RemoveAt(0);
            position.Add(NetworkTime.time, transform.position);

        Movement(picp);
        CmdApplyMovement(picp, NetworkTime.predictedTime, GetComponent<NetworkIdentity>());
    }

    [Command]
    void CmdApplyMovement(PlayerInput_CP input_CP, double predictedTime , NetworkIdentity ntd)
    {
        double delta = NetworkTime.time - predictedTime;

        Movement(input_CP);

        CheckForReconcilation(ntd , input_CP.timestamp , transform.position);
    }

    [ClientRpc]
    public void CheckForReconcilation(NetworkIdentity target , double time , Vector3 serverPosition)
    {
        clientthen.position = position[time];
        server.position = serverPosition;
        
        if (position.Count != 0)
        {
            if (Vector3.Distance(serverPosition, target.GetComponent<Cp_Controller>().position[time]) > .2f)
            {
                cc.enabled = false;
                target.transform.position = serverPosition;
                Debug.Log("test");
                cc.enabled = true;
            }
        }
    }
}

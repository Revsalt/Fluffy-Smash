using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ServerAuthoritativeTransform : NetworkBehaviour
{
    [SerializeField] ClientInput[] inputBuffer;
    [SerializeField] MovementResult[] stateBuffer;

    [SerializeField] private MovementResult[] stateBuffer_server;
    [SerializeField] private Queue<ClientInput> inputQueue;

    [SerializeField] private MovementResult latestServerState;
    [SerializeField] private MovementResult lastProcessedState;

    private void Start()
    {
        stateBuffer = new MovementResult[TickRate.BUFFER_SIZE];
        inputBuffer = new ClientInput[TickRate.BUFFER_SIZE];

        stateBuffer_server = new MovementResult[TickRate.BUFFER_SIZE];
        inputQueue = new Queue<ClientInput>();

        if (isLocalPlayer)
        {
            TickRate.Instance.OnTick += H_Tick_client;
        }

        if (isServer)
        {
            TickRate.Instance.OnTick += H_Tick_server;
        }

    }

    [ServerCallback]
    void H_Tick_server()
    {
        // Process the input queue
        int bufferIndex = -1;
        while (inputQueue.Count > 0)
        {
            ClientInput inputPayload = inputQueue.Dequeue();

            bufferIndex = inputPayload.tick % TickRate.BUFFER_SIZE;

            MovementResult statePayload = GetComponent<PlayerController>().ResultantMovement(inputPayload);
            stateBuffer_server[bufferIndex] = statePayload;
        }

        if (bufferIndex != -1)
        {
            SendToClient(GetComponent<NetworkIdentity>().connectionToClient , stateBuffer_server[bufferIndex]);
            SendToClients(GetComponent<NetworkIdentity>() , stateBuffer_server[bufferIndex]);
        }
    }

    [ClientCallback]
    void H_Tick_client()
    {
        if (!latestServerState.Equals(default(MovementResult)) &&
            (lastProcessedState.Equals(default(MovementResult)) ||
            !latestServerState.Equals(lastProcessedState)))
        {
            HandleServerReconciliation();
        }

        int bufferIndex = TickRate.Instance.currentTick % TickRate.BUFFER_SIZE;

        // Add payload to inputBuffer
        ClientInput inputPayload = new ClientInput();
        inputPayload.tick = TickRate.Instance.currentTick;

        inputPayload.movementAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        inputPayload.cameraRotation = GetComponent<PlayerController>().piviot_M.transform.localRotation;
        inputPayload.inputs = new bool[4] { Input.GetKey(KeyCode.Space) , Input.GetKey(KeyCode.LeftShift) , Input.GetMouseButton(0) , Input.GetMouseButton(1) };

        inputBuffer[bufferIndex] = inputPayload;

        // Add payload to stateBuffer
        stateBuffer[bufferIndex] = GetComponent<PlayerController>().ResultantMovement(inputPayload);

        // Send input to server
        SendInput(inputPayload);
    }

    [TargetRpc]
    public void SendToClient(NetworkConnection target , MovementResult statePayload)
    {
        target.identity.GetComponent<ServerAuthoritativeTransform>().latestServerState = statePayload;
    }

    [ClientRpc]
    public void SendToClients(NetworkIdentity ntd, MovementResult statePayload)
    {
        if (ntd.isLocalPlayer) return;

        ntd.transform.position = statePayload.position;
        ntd.GetComponent<PlayerController>().transform.localRotation = statePayload.rotation;
    }

    [Command]
    void SendInput(ClientInput ci, NetworkConnectionToClient sender = null)
    {
        sender.identity.gameObject.GetComponent<ServerAuthoritativeTransform>().inputQueue.Enqueue(ci);
    }

    void HandleServerReconciliation()
    {
        lastProcessedState = latestServerState;

        int serverStateBufferIndex = latestServerState.tick % TickRate.BUFFER_SIZE;

        float positionError = Vector3.Distance(latestServerState.position, stateBuffer[serverStateBufferIndex].position);

        if (positionError > .3f)
        {
            Debug.Log("We have to reconcile bro");
            // Rewind & Replay

            GetComponent<CharacterController>().enabled = false;
            GetComponent<CharacterController>().transform.position = latestServerState.position;
            GetComponent<CharacterController>().enabled = true;

            // Update buffer at index of latest server state
            stateBuffer[serverStateBufferIndex] = latestServerState;

            // Now re-simulate the rest of the ticks up to the current tick on the client
            int tickToProcess = latestServerState.tick + 1;

            while (tickToProcess < TickRate.Instance.currentTick)
            {
                int bufferIndex = tickToProcess % TickRate.BUFFER_SIZE;

                // Process new movement with reconciled state
                MovementResult statePayload = GetComponent<PlayerController>().ResultantMovement(inputBuffer[bufferIndex]);

                // Update buffer with recalculated state
                stateBuffer[bufferIndex] = statePayload;

                tickToProcess++;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var item in stateBuffer)
        {
            Gizmos.DrawWireSphere(item.position, 0.1f);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(latestServerState.position, 0.3f);
    }
}

[Serializable]
public struct ClientInput
{
    public int tick;
    public Vector2 movementAxis;
    public Quaternion cameraRotation;
    public bool[] inputs;
}

[Serializable]
public struct MovementResult
{
    public int tick;
    public Vector3 position;
    public Quaternion rotation;
}

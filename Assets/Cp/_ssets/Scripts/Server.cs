using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Server : NetworkBehaviour
{
    private float timer;
    private int currentTick;
    public float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 60f;
    private const int BUFFER_SIZE = 1024;

    private StatePayload[] stateBuffer;
    private Queue<InputPayload> inputQueue;

    void Start()
    {
        enabled = isServer;

        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

        stateBuffer = new StatePayload[BUFFER_SIZE];
        inputQueue = new Queue<InputPayload>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        while (timer >= minTimeBetweenTicks)
        {
            timer -= minTimeBetweenTicks;
            HandleTick();
            currentTick++;
        }
    }

    public void OnClientInput(InputPayload inputPayload)
    {
        inputQueue.Enqueue(inputPayload);
    }

    [ClientRpc] // change to trg rpc later
    public void SendToClient(StatePayload statePayload, NetworkIdentity target)
    {
            target.GetComponent<Client>().OnServerMovementState(statePayload);
    }

    void HandleTick()
    {
        // Process the input queue
        int bufferIndex = -1;
        while(inputQueue.Count > 0)
        {
            InputPayload inputPayload = inputQueue.Dequeue();

            bufferIndex = inputPayload.tick % BUFFER_SIZE;

            StatePayload statePayload = GetComponent<PlayerController>().Movement(inputPayload , minTimeBetweenTicks);
            stateBuffer[bufferIndex] = statePayload;
        }

        if (bufferIndex != -1)
        {
            SendToClient(stateBuffer[bufferIndex], GetComponent<NetworkIdentity>());
        }
    }
}

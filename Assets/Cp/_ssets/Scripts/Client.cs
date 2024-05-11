using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[System.Serializable]
public struct InputPayload
{
    public int tick;
    public byte bools;
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public struct StatePayload
{
    public int tick;
    public Vector3 position;
}

public class Client : NetworkBehaviour
{
    // Shared
    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 60f;
    private const int BUFFER_SIZE = 2048;

    // Client specific
    private StatePayload[] stateBuffer;
    private InputPayload[] inputBuffer;
    private StatePayload latestServerState;
    private StatePayload lastProcessedState;
    private float horizontalInput;
    private float verticalInput;


    void Start()
    {
        enabled = isLocalPlayer;

        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

        stateBuffer = new StatePayload[BUFFER_SIZE];
        inputBuffer = new InputPayload[BUFFER_SIZE];
    }

    bool isJumpPressed = false;
    bool isDashPressed = false;

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        timer += Time.deltaTime;

        while (timer >= minTimeBetweenTicks)
        {
            timer -= minTimeBetweenTicks;
            HandleTick();
            currentTick++;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJumpPressed = true;
        }

        if (Input.GetMouseButtonDown(1))
        {
            isDashPressed = true;
        }
    }

    public void OnServerMovementState(StatePayload serverState)
    {
        latestServerState = serverState;
    }

    [Command(channel = Channels.Unreliable)]
    public void SendToServer(InputPayload inputPayload , NetworkConnectionToClient sender = null)
    {
        sender.identity.GetComponent<Server>().OnClientInput(inputPayload);
    }

    void HandleTick()
    {
        if (!latestServerState.Equals(default(StatePayload)) &&
            (lastProcessedState.Equals(default(StatePayload)) ||
            !latestServerState.Equals(lastProcessedState)))
        {
            HandleServerReconciliation();
        }

        int bufferIndex = currentTick % BUFFER_SIZE;

        // Add payload to inputBuffer
        InputPayload inputPayload = new InputPayload();
        inputPayload.tick = currentTick;
        Vector3 direc = horizontalInput * transform.right + verticalInput * transform.forward;

        inputPayload.x = RoundOff(direc.x);
        inputPayload.y = RoundOff(direc.y);
        inputPayload.z = RoundOff(direc.z);

        inputBuffer[bufferIndex] = inputPayload;

        int bits = (isJumpPressed ? 1 : 0) | (isDashPressed ? 2 : 0);
        inputPayload.bools = (byte)bits;

        // Add payload to stateBuffer
        stateBuffer[bufferIndex] = GetComponent<PlayerController>().Movement(inputPayload , minTimeBetweenTicks);

        // Send input to server
        SendToServer(inputPayload);

        isJumpPressed = false; // rest jump press bec this is a differet tick rate
        isDashPressed = false; // rest dash press bec this is a differet tick rate
    }

    public float RoundOff(float i)
    {
        return Mathf.Round(i * 10) / 10;
    }

    void HandleServerReconciliation()
    {
        lastProcessedState = latestServerState;

        int serverStateBufferIndex = latestServerState.tick % BUFFER_SIZE;
        float positionError = Vector3.Distance(latestServerState.position, stateBuffer[serverStateBufferIndex].position);

        if (positionError > 0.001f)
        {
            GameObject g = (GameObject)Instantiate(Resources.Load("reconcile"), latestServerState.position, Quaternion.identity, null);
            GameObject k = (GameObject)Instantiate(Resources.Load("myClientPos"), stateBuffer[serverStateBufferIndex].position, Quaternion.identity, null);
            Destroy(g, 20);
            Destroy(k, 20);
            Debug.Log("We have to reconcile");
            // Rewind & Replay
            GetComponent<CharacterController>().enabled = false;
            transform.position = latestServerState.position;
            GetComponent<CharacterController>().enabled = true;

            // Update buffer at index of latest server state
            stateBuffer[serverStateBufferIndex] = latestServerState;

            // Now re-simulate the rest of the ticks up to the current tick on the client
            int tickToProcess = latestServerState.tick + 1;

            while (tickToProcess < currentTick)
            {
                int bufferIndex = tickToProcess % BUFFER_SIZE;

                // Process new movement with reconciled state
                StatePayload statePayload = GetComponent<PlayerController>().Movement(inputBuffer[bufferIndex] , minTimeBetweenTicks);

                // Update buffer with recalculated state
                stateBuffer[bufferIndex] = statePayload;

                tickToProcess++;
            }
        }
    }
}

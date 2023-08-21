using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public struct InputPayload
{
    public int tick;
    public Vector3 inputVector;
    public bool input;
}

public struct StatePayload
{
    public int tick;
    public Vector3 position;
}

public class Client_tr : NetworkBehaviour
{
    public static Client_tr Instance;

    // Shared
    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 30f;
    private const int BUFFER_SIZE = 1024;

    // Client specific
    private StatePayload[] stateBuffer;
    private InputPayload[] inputBuffer;
    private StatePayload latestServerState;
    private StatePayload lastProcessedState;
    private float horizontalInput;
    private float verticalInput;

    [SerializeField] Transform target;
    [SerializeField] GameObject[] mycam;

    CharacterController cc;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        Instance = this;

        target.SetParent(null);
    }

    void Start()
    {
        foreach (var item in mycam)
        {
            if (isLocalPlayer)
            {
                item.SetActive(true);
            }
        }

        //Create random color
        Color col1 = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
        //Find mesh from game objects
        Renderer mesh1 = GetComponent<Renderer>();
        //Change colors of meshes
        mesh1.material.color = col1;

        if (isLocalPlayer)
            enabled = true;
        else
            enabled = false;

        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

        stateBuffer = new StatePayload[BUFFER_SIZE];
        inputBuffer = new InputPayload[BUFFER_SIZE];
    }

    void Update()
    {
        target.transform.position = Vector3.Lerp(target.transform.position , transform.position , 6 * Time.deltaTime);
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");


        //shared
        timer += Time.deltaTime;

        while (timer >= minTimeBetweenTicks)
        {
            timer -= minTimeBetweenTicks;
            HandleTick();
            currentTick++;
        }
    }

    public void OnServerMovementState(StatePayload serverState)
    {
        latestServerState = serverState;
    }

    [Command(requiresAuthority =false)]
    void SendToServer(InputPayload inputPayload)
    {
        Server_tr.Instance.OnClientInput(inputPayload);
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
        inputPayload.inputVector = new Vector3(horizontalInput, 0, verticalInput);
        inputPayload.input = Input.GetKey(KeyCode.E);

        inputBuffer[bufferIndex] = inputPayload;

        // Add payload to stateBuffer
        stateBuffer[bufferIndex] = ProcessMovement(inputPayload);

        // Send input to server
        SendToServer(inputPayload);
    }

    int cooldown = 15;
    int my_tick = 0;
    StatePayload ProcessMovement(InputPayload input)
    {
        // Should always be in sync with same function on Server
        cc.Move(input.inputVector * Server_tr.Instance.p_speed * minTimeBetweenTicks);

        if (input.input && input.tick > my_tick + cooldown)
        {
            my_tick = input.tick;
            Dash();
        }

        void Dash()
        {
            cc.Move(input.inputVector * 50 * minTimeBetweenTicks);
        }

        return new StatePayload()
        {
            tick = input.tick,
            position = transform.position,
        };
    }

    void HandleServerReconciliation()
    {
        lastProcessedState = latestServerState;

        int serverStateBufferIndex = latestServerState.tick % BUFFER_SIZE;

        float positionError = Vector3.Distance(latestServerState.position, stateBuffer[serverStateBufferIndex].position);

        if (positionError > 0.001f)
        {
            Debug.Log("We have to reconcile bro");
            // Rewind & Replay
            transform.position = latestServerState.position;

            // Update buffer at index of latest server state
            stateBuffer[serverStateBufferIndex] = latestServerState;

            // Now re-simulate the rest of the ticks up to the current tick on the client
            int tickToProcess = latestServerState.tick + 1;

            while (tickToProcess < currentTick)
            {
                int bufferIndex = tickToProcess % BUFFER_SIZE;

                // Process new movement with reconciled state
                StatePayload statePayload = ProcessMovement(inputBuffer[bufferIndex]);

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Server_tr : NetworkBehaviour
{
    public static Server_tr Instance;

    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 30f;
    private const int BUFFER_SIZE = 1024;

    private StatePayload[] stateBuffer;
    private Queue<InputPayload> inputQueue;

    public float latency = 0.02f, p_speed = 30;

    CharacterController cc;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        Instance = this;
    }

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

    [ClientRpc]
    public void SendToClient(StatePayload statePayload)
    {
        Client_tr.Instance.OnServerMovementState(statePayload);
    }

    void HandleTick()
    {
        // Process the input queue
        int bufferIndex = -1;
        while (inputQueue.Count > 0)
        {
            InputPayload inputPayload = inputQueue.Dequeue();

            bufferIndex = inputPayload.tick % BUFFER_SIZE;

            StatePayload statePayload = ProcessMovement(inputPayload);
            stateBuffer[bufferIndex] = statePayload;
        }

        if (bufferIndex != -1)
        {
            SendToClient(stateBuffer[bufferIndex]);
        }
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
            cc.Move(input.inputVector * 20 * minTimeBetweenTicks);
        }

        return new StatePayload()
        {
            tick = input.tick,
            position = transform.position,
        };
    }
}
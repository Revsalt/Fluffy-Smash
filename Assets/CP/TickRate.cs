using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TickRate : MonoBehaviour
{
    public static TickRate Instance;

    public float timer;
    public int currentTick;
    [HideInInspector] public float minTimeBetweenTicks;
    [HideInInspector] public const float SERVER_TICK_RATE = 60f;
    [HideInInspector] public const int BUFFER_SIZE = 1024;

    public Action OnTick;
    public Action OnStartTickRate;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;
        if (OnStartTickRate != null)
            OnStartTickRate.Invoke();
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

    void HandleTick()
    {
        if (OnTick != null)
            OnTick.Invoke();
    }

    public static float GetMinTimeBetweenTicks()
    {
        return (1f / SERVER_TICK_RATE);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(Screen.width - 100, 0, 80, 20) , (currentTick / 60).ToString() + "| tick / time");
    }
}

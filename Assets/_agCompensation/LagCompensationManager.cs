using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Mirror;

/// <summary>
/// The main class for controlling lag compensation
/// </summary>
public class LagCompensationManager : MonoBehaviour
{
    private void FixedUpdate()
    {
        if (NetworkServer.active)
            AddFrames();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void FirstInitialize()
    {
        GameObject go = new GameObject();
        go.name = "LagCompensationManager";
        go.AddComponent<LagCompensationManager>();
        DontDestroyOnLoad(go);
    }

    public static readonly List<TrackedObject> SimulationObjects = new List<TrackedObject>();

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use SimulationObjects instead", false)]
    public static List<TrackedObject> simulationObjects => SimulationObjects;

    public static void StartSimulation(float secondsAgo)
    {
        StartSimulation(secondsAgo, SimulationObjects);
    }

    public static void StartSimulation(float secondsAgo, IList<TrackedObject> simulatedObjects)
    {
        if (!NetworkServer.active)
            return;

        for (int i = 0; i < simulatedObjects.Count; i++)
        {
            simulatedObjects[i].ReverseTransform(secondsAgo);
        }
    }

    public static void StopSimulation()
    {
        StopSimulation(SimulationObjects);
    }

    public static void StopSimulation(IList<TrackedObject> simulatedObjects)
    {
        if (!NetworkServer.active)
            return;


        for (int i = 0; i < simulatedObjects.Count; i++)
        {
            simulatedObjects[i].ResetStateTransform();
        }
    }

    internal static void AddFrames()
    {
        for (int i = 0; i < SimulationObjects.Count; i++)
        {
            SimulationObjects[i].AddFrame();
        }
    }
}


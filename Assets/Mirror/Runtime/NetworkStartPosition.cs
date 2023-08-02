using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Mirror
{
    /// <summary>Start position for player spawning, automatically registers itself in the NetworkManager.</summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkStartPosition")]
    [HelpURL("https://mirror-networking.gitbook.io/docs/components/network-start-position")]
    public class NetworkStartPosition : MonoBehaviour
    {
        public void Awake()
        {
            Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit);
            transform.position = hit.point + Vector3.up;

            NetworkManager.RegisterStartPosition(transform);
        }

        public void OnDestroy()
        {
            NetworkManager.UnRegisterStartPosition(transform);
        }
    }
}

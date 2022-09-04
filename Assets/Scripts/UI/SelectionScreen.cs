using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionScreen : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (SteamLobby.Instance)
        {
            if (gameObject.activeSelf == SteamLobby.Instance.IsInLobby)
            {
                gameObject.SetActive(!SteamLobby.Instance.IsInLobby);
            }
        }
    }
}

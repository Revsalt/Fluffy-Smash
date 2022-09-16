using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public void Start()
    {
        gameObject.SetActive(!NetworkRoomManager.singleton.GetComponent<NetworkManagerHUD>());
    }

    public void LoadSpecificScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
}

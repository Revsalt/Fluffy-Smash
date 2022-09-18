using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : NetworkBehaviour
{
    public GameObject powerUp;
    GameObject myPowerUp;
    
    [ServerCallback]
    public void Start()
    {
        SpawmPowerUp();
    }

    bool hasPowerUp_ = false;

    [ServerCallback]
    public void Update()
    {
        if (!hasPowerUp() && !hasPowerUp_)
        {
            StartCoroutine(spawnPowerUp());
            hasPowerUp_ = true;
        }
    }

    IEnumerator spawnPowerUp()
    {
        yield return new WaitForSeconds(7);
        SpawmPowerUp();
        hasPowerUp_ = false;
    }

    void SpawmPowerUp()
    {
        myPowerUp = Instantiate(powerUp, transform.position, Quaternion.identity, null);
        NetworkServer.Spawn(myPowerUp);
        UpdatePos(myPowerUp.GetComponent<NetworkIdentity>(), myPowerUp.transform.position);
    }

    [ClientRpc]
    void UpdatePos(NetworkIdentity ntd , Vector3 pos)
    {
        ntd.gameObject.transform.position = pos;
    }

    bool hasPowerUp()
    {
        return myPowerUp;
    }
}

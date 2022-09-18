using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : NetworkBehaviour
{
    // Start is called before the first frame update
    public BasePowerUp[] powerUp;
    BasePowerUp ActualPowerup;
    public GameObject Particles;
    void Start()
    {
        //int RandomNumber = Random.Range(0, powerUp.Length);
        ActualPowerup = powerUp[1];
        Debug.Log(ActualPowerup.PowerupEffect);

    }
    public BasePowerUp GetPowerUp()
    {
        return ActualPowerup;
    }

    public void NetworkDestroy()
    {
        CmdDestroy(gameObject);
    }

    private void OnDestroy()
    {
        if (!this.gameObject.scene.isLoaded) return;

        GameObject particle = Instantiate(Particles, this.transform.position, Quaternion.identity);
        Destroy(particle, 2f);
    }

    [Command(requiresAuthority = false)]
    void CmdDestroy(GameObject g)
    {
        NetworkServer.Destroy(g);
    }

    public void Update()
    {
        transform.position = transform.position + new Vector3(0, (Mathf.Sin(Time.time * 5) / 2) * Time.deltaTime, 0);
    }
}

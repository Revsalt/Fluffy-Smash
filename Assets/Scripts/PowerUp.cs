using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
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
    private void OnDestroy()
    {
        GameObject particle = Instantiate(Particles, this.transform.position, Quaternion.identity);
        Destroy(particle, 2f);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

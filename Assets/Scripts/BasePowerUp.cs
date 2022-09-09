using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New powerup",menuName = "Powerup")]
public class BasePowerUp : ScriptableObject
{
    public string Name;
    public string Description;
    public enum effect {Speed,Jump,Cooldown}
    public effect PowerupEffect;
    public float EffectDuration;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
//using static UnityEditor.Progress;

public class VisualizeAbilites : NetworkBehaviour
{

    // DO NOT CHNAGE THE ORDER OF THE CHILDREN IN THE SPAWNED OBJECT

    PlayerController pc;

    List<AbilityDisplayBoard> abilities = new List<AbilityDisplayBoard>();

    void Start()
    {
        if (!isLocalPlayer) { Destroy(this); return; }

        pc = GetComponent<PlayerController>();

        GameObject t = (GameObject)Instantiate(Resources.Load("AbilityVisualization"), Vector3.zero, Quaternion.identity, null);

        foreach (var item in new Ability[3] { pc.ability0, pc.ability1, pc.ability_tag })
        {
            item.coolDown_current_value = item.coolDown;

            GameObject ab = (GameObject)Instantiate(Resources.Load("AbilityDisplay"), t.transform.GetChild(0).transform);

            AbilityDisplayBoard adb = new AbilityDisplayBoard
            {
                ab = item,
                coolDown_ = ab.transform.GetChild(0).GetComponent<Text>(),
                name_ = ab.transform.GetChild(1).GetComponent<Text>(),
                coolDownSlider_ = ab.transform.GetChild(2).GetComponent<Transform>()
            };

            abilities.Add(adb);
        }

    }

    void Update()
    {
        if (abilities == null) return;

        foreach (var item in abilities)
        {
            if ((Mathf.Round(item.ab.coolDown_current_value * 10) / 10) == item.ab.coolDown)
            {
                item.coolDown_.text = "READY";
                item.coolDownSlider_.localScale = new Vector3(1,1,1);
            }
            else
            {
                float value = (Mathf.Round(item.ab.coolDown_current_value * 10) / 10);
                item.coolDown_.text = value.ToString();
                item.coolDownSlider_.localScale = Vector3.Lerp(item.coolDownSlider_.localScale , new Vector3(1, value / item.ab.coolDown, 1) , 8 * Time.deltaTime);
            }

            item.name_.text = item.ab.abilityName;

            if (item.ab.coolDown == 0)
                item.coolDown_.text = "NO COOlDOWN";
        }
    }

    private void OnGUI()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            foreach (var item in abilities)
            {
                item.coolDown_.transform.parent.gameObject.SetActive(false);
            }
        }
    }
}

class AbilityDisplayBoard
{
    public Ability ab;
    public Text coolDown_;
    public Transform coolDownSlider_;
    public Text name_;
}

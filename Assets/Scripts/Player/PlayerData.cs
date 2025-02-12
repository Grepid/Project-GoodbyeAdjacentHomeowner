using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    //Add Static variables for things like "levels" or I guess list of Items of what the player owns

    //Then add member variables for things that'd be current (can't think of what it'd be)
    private void Awake()
    {
        Player.SetData(this);
    }
}

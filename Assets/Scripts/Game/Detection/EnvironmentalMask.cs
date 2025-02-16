using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentalMask : InteractionBase
{

    public int maskLevel;
    public float fixTime;
    private float alpha;
    public bool isOn { get; private set; }
    public bool fixing { get; private set; }
    public void SetState(bool value)
    {
        if (isOn == value) return;
        isOn = value;
        if (value)
        {
            SoundDetection.instance.AddMask(this);
        }
        else
        {
            SoundDetection.instance.RemoveMask(this);
        }

                
    }
    private void Update()
    {
        if (!fixing) return;
        if (!isOn) return;
        if(alpha >= 1)
        {
            SetState(false);
            theguy.FinishedFixingMask();
            alpha = 0;
        }
        alpha += Time.deltaTime / fixTime;
    }

    protected override void OnInteract()
    {
        SetState(!isOn);
    }
    BaseEnemy theguy;
    public void SetFixing(bool value, BaseEnemy enemy)
    {
        theguy = enemy;
        if (fixing == value) return;
        fixing = value;
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : InteractionBase
{
    [SerializeField]
    private AutoMove moveableCall;

    public float suspicionPercentToAdd = 10;

    [SerializeField]
    private DoorKey key;

    public bool Locked
    {
        get
        {
            if (key == null) return false;
            return !key.Obtained;
        }
    }

    protected override void OnInteract()
    {
        if (moveableCall.active) return;
        if (Locked)
        {
            TriedToOpenLocked();
            return;
        }
        SoundDetection.instance?.AddTemporarySuspicionPercent(suspicionPercentToAdd);
        OpenDoor(moveableCall.alpha < 1);
    }
    public void OpenDoor(bool value)
    {
        if (value && moveableCall.alpha == 0) moveableCall.Pull();
        else if (!value && moveableCall.alpha == 1)moveableCall.Release();
    }
    private void TriedToOpenLocked()
    {

    }
}

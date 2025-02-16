using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericActions : InteractionBase
{
    public bool Repeats
    {
        get
        {
            return repeats;
        }
        set
        {
            repeats = value;
            if (value) hasReachedEnd = false;
        }
    }
    public bool repeats;
    [SerializeField]
    private UnityEvent[] InteractEvents;
    bool hasReachedEnd;

    int iCount;
    protected override void OnInteract()
    {
        if (iCount == 0 && !repeats && hasReachedEnd) return;
        InteractEvents[iCount]?.Invoke();
        iCount++;
        if(iCount == InteractEvents.Length)
        {
            iCount = 0;
            hasReachedEnd = true;
        }
    }

    [ContextMenu("FlipRepeats")]
    private void FlipRepeats()
    {
        Repeats = !Repeats;
    }
}

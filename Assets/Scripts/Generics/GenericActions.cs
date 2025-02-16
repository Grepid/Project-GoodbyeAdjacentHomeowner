using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericActions : InteractionBase
{
    [SerializeField]
    private UnityEvent InteractEvent;
    protected override void OnInteract()
    {
        InteractEvent?.Invoke();   
    }
}

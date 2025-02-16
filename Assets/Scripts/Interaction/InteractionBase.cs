using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractionBase : MonoBehaviour
{
    [SerializeField]
    private bool canInteract;
    public bool CanInteract => canInteract;
    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }
    public void TryInteract()
    {
        if (!canInteract) return;
        OnInteract();
    }
    protected abstract void OnInteract();
}

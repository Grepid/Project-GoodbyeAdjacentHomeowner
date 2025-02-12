using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIElement : MonoBehaviour
{
    private enum Error { RectNull }
    public RectTransform rt { get; private set; }

    public bool canBeForceClosed;
    public bool interactable;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (rt == null)
        {
            PostError(Error.RectNull);
            return;
        }
        Init();
    }
    private void Init()
    {
        UIController.AddElementToStack(this);
    }
    private void PostError(Error error)
    {
        switch (error)
        {
            case Error.RectNull:
                Debug.LogWarning($"Trying to initialise {gameObject.name} as a UIElement without having a rect transform.");
                enabled = false;
                break;
        }
    }
    public void DestroyElement()
    {
        UIController.DestroyElement(this);
    }
}

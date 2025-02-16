using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class MoveableObject : MonoBehaviour
{
    public enum HookMode { Move, Rotate, Spin }
    public HookMode[] hookMode;

    private float heldAlpha;

    public Vector3 SpinAmount;
    public bool SpinDoesntReset;
    private float lastAlpha = 0;

    private Quaternion startQuat;
    public Quaternion intendedQuat;

    private Vector3 startLocation;
    public Vector3 intendedMoveLocation;

    public UnityEvent OnActionMaxed, OnActionZeroed;
    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        startLocation = transform.localPosition;
        startQuat = transform.localRotation;
    }

    /// <summary>
    /// Sets the desired quaternion rotation to be pulled to (Has to be on a function as
    /// you cannot edit Quaternions in the inspector)
    /// </summary>
    [ContextMenu("Set Intended Rotation")]
    public void SetIntendedQuat()
    {
        intendedQuat = transform.localRotation;
    }

    [ContextMenu("Set Intended Position")]
    public void SetIntendedMoveLocation()
    {
        intendedMoveLocation = transform.localPosition;
    }

    /// <summary>
    /// Adjusts the object based on the newest alpha value from the Hook object
    /// </summary>
    /// <param name="alpha"></param>
    public void AdjustObject(float alpha)
    {
        if (hookMode.Contains(HookMode.Move))
        {
            transform.localPosition = Vector3.Lerp(startLocation, intendedMoveLocation, alpha);
        }
        if (hookMode.Contains(HookMode.Rotate))
        {
            transform.localRotation = Quaternion.Lerp(startQuat, intendedQuat, alpha);
        }
        if (hookMode.Contains(HookMode.Spin))
        {
            Vector3 spinAmount = (SpinAmount * alpha) - (SpinAmount * lastAlpha);
            transform.Rotate(spinAmount);
            lastAlpha = alpha;
        }
    }
}

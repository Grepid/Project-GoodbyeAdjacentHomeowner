using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AutoMove : MonoBehaviour
{
    public bool active { get; private set; }
    public List<MoveableObject> linkedObjects = new List<MoveableObject>();
    public float pullTime;
    float usingTime;
    public float alpha { get; private set; }
    float direction = 1;
    public UnityEvent OnReachedMax, OnReachedZero;

    // Update is called once per frame
    void Update()
    {
        if (!active) return;

        alpha = Mathf.Clamp01(alpha + (Time.deltaTime / usingTime) * direction);
        foreach (var w in linkedObjects)
        {
            w.AdjustObject(alpha);
        }
        if (alpha == 1 || alpha == 0)
        {
            if (alpha == 1)
            {
                //print("auto maxed");
                OnReachedMax?.Invoke();
            }
            if (alpha == 0)
            {
                OnReachedZero?.Invoke();
                //print("auto zero");
            }
            active = false;
        }
    }

    public void Pull(float speed)
    {
        usingTime = speed;
        direction = 1;
        alpha = 0;
        active = true;
    }
    public void Pull()
    {
        Pull(pullTime);
    }
    public void Release(float speed)
    {
        usingTime = speed;
        direction = -1;
        alpha = 1;
        active = true;
    }
    public void Release()
    {
        Release(pullTime);
    }
    public void Stop()
    {
        active = false;
    }
    public void FlipState()
    {
        if(alpha > 0) Release();
        else Pull();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Task : MonoBehaviour
{
    [SerializeField]
    float taskTime;
    public float TaskTime { get; private set; }
    [SerializeField]
    private UnityEvent OnStart, OnLeave, OnComplete;

    public BaseEnemy occupant;
    private float completion;

    public bool completed;

    

    private void Awake()
    {
        TaskTime = taskTime;
    }
    private void Update()
    {
        if (occupant == null) return;
        completion += Time.deltaTime / TaskTime;
        if(completion >= 1)
        {
            CompleteTask();
        }
    }

    public void ResetTask()
    {
        occupant = null;
        completion = 0;
        completed = false;
    }

    public void StartTask(BaseEnemy caller)
    {
        if (occupant != null) return;
        occupant = caller;
        OnStart?.Invoke();
    }
    public void LeaveTask(BaseEnemy caller)
    {
        if (occupant != caller) return;
        occupant = null;
        OnLeave?.Invoke();
    }
    public void CompleteTask()
    {
        completed = true;
        OnComplete?.Invoke();
        occupant.TaskComplete();
        occupant = null;
    }
}

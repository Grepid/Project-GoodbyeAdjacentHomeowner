using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : ControllerBase
{
    public static LevelController Instance;
    public Transform TaskHolder;
    public List<Task> Tasks;
    private void Awake()
    {
        
    }
    private void AssignTasks()
    {
        foreach(Transform t in TaskHolder)
        {
            Task task = t.GetComponent<Task>();
            if (t == null) continue;
            Tasks.Add(task);
        }
    }

    public override void Initialise()
    {
        Instance = this;
        AssignTasks();
    }
}

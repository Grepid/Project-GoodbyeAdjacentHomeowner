using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;
    public Transform TaskHolder;
    public List<Task> Tasks;
    private void Awake()
    {
        Instance = this;
        AssignTasks();
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
}

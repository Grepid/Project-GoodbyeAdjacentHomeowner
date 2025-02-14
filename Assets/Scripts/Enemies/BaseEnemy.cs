using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Grepid.BetterRandom;

public enum EnemyState {Idle,MovingToTask,DoingTask,MovingToInvestigateNoise,InvestigatingNoise,SpottedPlayer}
public class BaseEnemy : MonoBehaviour
{
    // Varibles for enemies
    public NavMeshAgent agent;

    public float FOV;
    public float detectionDistance;
    //Maybe make the ability to do tasks both a Player and Enemy thing from a common base class but idk if 
    //the player will ever need to do a task
    private Task currentTask, DesiredTask;

    public EnemyState state { get; private set; }

    private void Start()
    {
        ProceedToTask(ChooseRandomTask());
    }

    public void Update()
    {
        TryChangeState();
    }
    private bool CanSeePlayer()
    {
        Vector3 direction = (Player.Controller.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, direction);
        if(angle <= (FOV / 2f) && Vector3.Distance(Player.Controller.transform.position,transform.position) <= detectionDistance)
        {
            if(Physics.Raycast(transform.position,direction,out RaycastHit hit))
            {
                if(hit.collider.gameObject == Player.Controller.gameObject)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private void TryChangeState()
    {
        
        switch (state)
        {
            case EnemyState.Idle:
                //Idk if this will have a use anyway atm
                break;

            case EnemyState.MovingToTask:
                //if sees player break
                //if gets to task break and call task 
                //if noise is over threshold investigate
                if (PlayerSensed())
                {
                    break;
                }
                if (agent.remainingDistance < 0.5f)
                {
                    DesiredTask.StartTask(this);
                    currentTask = DesiredTask;
                    state = EnemyState.DoingTask;
                    break;
                }
                break;

            case EnemyState.DoingTask:
                //if sees player break
                //if noise is over threshold investigate
                //get the called task to call complete in here when completed
                //if breaks early from task, call task stopped or something
                if (PlayerSensed())
                {
                    currentTask.LeaveTask(this);
                    currentTask = null;
                    break;
                }
                break;

            case EnemyState.MovingToInvestigateNoise:
                //if sees player break
                //if get to area, start investigating noise itself (looking around and stuff)
                if (PlayerSensed())
                {
                    break;
                }
                if (agent.remainingDistance < 0.5f)
                {
                    state = EnemyState.InvestigatingNoise;
                    timeOfStartInvestigation = Time.time;
                    break;
                }

                break;

            case EnemyState.InvestigatingNoise:
                //if sees player break
                //looks around for x seconds then gets back to task if it wasnt complete, or else go to a new task
                if (PlayerSensed())
                {
                    break;
                }
                transform.Rotate((Vector3.up * 360 / 5f) * Time.deltaTime);
                if (Time.time >= timeOfStartInvestigation + 5f)
                {
                    TryResumeTask();
                }
                break;

            case EnemyState.SpottedPlayer:

                //adds x permanent sus per second when spotted
                //if permanent sus is over y then plays game over cutscene
                if (SoundDetection.instance.IsTaggedAndCursed)
                {
                    print("GGS YOU LOSE");
                    break;
                }
                if (!CanSeePlayer())
                {
                    state = EnemyState.InvestigatingNoise;
                    timeOfStartInvestigation = Time.time;
                }
                SoundDetection.instance.AddPermanentSoundLevelPercent(50 * Time.deltaTime);
                break;


        }
    }
    private float timeOfStartInvestigation;
    private bool PlayerSensed()
    {
        if (CanSeePlayer())
        {
            PlayerSeen();
            return true;
        }
        if (SoundDetection.instance.IsDetected)
        {
            PlayerHeard();
            return true;
        }
        return false;
    }
    private void PlayerSeen()
    {
        state = EnemyState.SpottedPlayer;
        agent.SetDestination(Player.Controller.transform.position);
    }
    private void PlayerHeard()
    {
        state = EnemyState.MovingToInvestigateNoise;
        agent.SetDestination(Player.Controller.transform.position);
    }
    public void ProceedToTask(Task task)
    {
        state = EnemyState.MovingToTask;
        DesiredTask = task;
        agent.SetDestination(task.transform.position);
    }
    public Task ChooseRandomTask()
    {
        //Instead of looping in order, make selection truly random
        Task task = null;
        task = LevelController.Instance.Tasks.Find(t => !t.completed);
        if(task == null)
        {
            foreach(var ta in LevelController.Instance.Tasks)
            {
                ta.ResetTask();
            }
            task = LevelController.Instance.Tasks.Find(t => !t.completed);
        }
        return task;
    }
    public void TaskComplete()
    {
        ProceedToTask(ChooseRandomTask());
    }
    private void TryResumeTask()
    {
        if (DesiredTask.completed)
        {
            ProceedToTask(ChooseRandomTask());
            return;
        }
        ProceedToTask(DesiredTask);
    }
}

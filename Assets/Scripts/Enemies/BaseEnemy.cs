using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState {Idle,MovingToTask,DoingTask,MovingToInvestigateNoise,InvestigatingNoise,SpottedPlayer}
public class BaseEnemy : MonoBehaviour
{
    // Varibles for enemies
    public NavMeshAgent agent;

    public float FOV;
    public float detectionDistance;
    //Maybe make the ability to do tasks both a Player and Enemy thing from a common base class but idk if 
    //the player will ever need to do a task
    public Task task;

    public EnemyState state { get; private set; }

    private void Start()
    {
        ProceedToTask();
    }

    public void Update()
    {
        TryChangeState();
    }
    private bool CanSeePlayer()
    {
        Vector3 direction = (Player.Controller.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, direction);
        if(angle <= (FOV / 2) && Vector3.Distance(Player.Controller.transform.position,transform.position) <= detectionDistance)
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
                
                if(agent.remainingDistance < 0.5f)
                {
                    task.StartTask(this);
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
                    task.LeaveTask(this);
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
                if (PlayerSensed()) break;

                if (Time.time >= timeOfStartInvestigation + 5f)
                {
                    ProceedToTask();
                }
                break;

            case EnemyState.SpottedPlayer:

                //adds x permanent sus per second when spotted
                //if permanent sus is over y then plays game over cutscene
                if (!CanSeePlayer())
                {
                    state = EnemyState.MovingToInvestigateNoise;
                    agent.SetDestination(Player.Controller.transform.position);
                }
                SoundDetection.instance.AddPermanentSoundLevelPercent(50 * Time.deltaTime);
                if (SoundDetection.instance.IsTaggedAndCursed)
                {
                    print("GGS YOU LOSE");
                }
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
    public void ProceedToTask()
    {
        state = EnemyState.MovingToTask;
        agent.SetDestination(task.transform.position);
    }
    public void TaskComplete()
    {
        print("task completed");
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Grepid.BetterRandom;
using System.Linq;
using TMPro;

// TO-DO: MAKE A GOOD DEBUG SYSTEM

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
    public float walkingSpeed,investigatingSpeed;
    public List<Task> potentialTasks;
    public TextMeshProUGUI debugText;

    public EnemyState state { get; private set; }

    private void Start()
    {
        ProceedToTask(ChooseRandomTask());
        agent.speed = walkingSpeed;
    }
    bool init;
    private void OnEnable()
    {
        if (init) return;

        potentialTasks = Rand.ShuffleCollection(LevelController.Instance.Tasks).ToList();

        init = true;
    }
    
    public void Update()
    {
        TryChangeState();
        debugText.text = $"State: {state}";
        //Debug stuff
        //WorkOutSin();
        if (Input.GetKeyDown(KeyCode.L))
        {
            counter = 0;
        }
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
                //Maybe use Idle is a transition state where it wants to see what its next priority should be

                if (PlayerSensed())
                {
                    break;
                }

                //Do things like check if there is an environmental object on

                break;

            case EnemyState.MovingToTask:
                //if sees player break
                //if gets to task break and call task 
                //if noise is over threshold investigate


                if (PlayerSensed())
                {
                    break;
                }
                if(agent.hasPath && agent.remainingDistance < 0.5f)
                {
                    DesiredTask.StartTask(this);
                    agent.ResetPath();
                    currentTask = DesiredTask;
                    state = EnemyState.DoingTask;
                    break;
                }
                break;

            ///
            /// Doing Task
            ///
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
                    startingRotFromInvestigation = transform.eulerAngles.y;
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
                //transform.Rotate((Vector3.up * 360 / 5f) * Time.deltaTime); 

                Vector3 rot = transform.eulerAngles;
                rot.y = startingRotFromInvestigation + (90 * Mathf.Sin(Time.time - timeOfStartInvestigation));
                //print(Time.time - timeOfStartInvestigation);
                //print((90 * Mathf.Sin(Time.time - timeOfStartInvestigation)));
                print(Mathf.Sin(Time.time-timeOfStartInvestigation));
                transform.eulerAngles = rot;

                if (Time.time >= timeOfStartInvestigation + 5f)
                {
                    TryResumeTask();
                    agent.speed = walkingSpeed;
                }
                break;

            case EnemyState.SpottedPlayer:

                //adds x permanent sus per second when spotted
                //if permanent sus is over y then plays game over cutscene


                if (SoundDetection.instance.IsTaggedAndCursed)
                {
                    agent.ResetPath();
                    Player.Controller.SetPlayerControl(false);
                    break;
                }
                if (!CanSeePlayer())
                {
                    state = EnemyState.InvestigatingNoise;
                    timeOfStartInvestigation = Time.time;
                }
                SoundDetection.instance.AddPermanentSuspicionPercent(50 * Time.deltaTime);
                break;


        }
    }
    float startingRotFromInvestigation;
    private float timeOfStartInvestigation;
    private bool PlayerSensed()
    {
        if (CanSeePlayer())
        {
            PlayerSeen();
            agent.speed = walkingSpeed;
            return true;
        }
        if (SoundDetection.instance.IsDetected)
        {
            PlayerHeard();
            agent.speed = investigatingSpeed;
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
        Task task = null;
        if (potentialTasks.Count <= 0) potentialTasks = Rand.ShuffleCollection(LevelController.Instance.Tasks).ToList();

        task = Rand.RandFromCollection(potentialTasks);
        return task;
    }
    public void TaskComplete()
    {
        currentTask.ResetTask();
        potentialTasks.Remove(currentTask);
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
    float counter;
    private void WorkOutSin()
    {
        
        print(Mathf.Sin(counter));
        counter += Time.deltaTime;
    }
}

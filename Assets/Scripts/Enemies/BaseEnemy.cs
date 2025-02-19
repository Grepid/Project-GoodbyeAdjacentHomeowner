using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Grepid.BetterRandom;
using System.Linq;
using TMPro;
using AudioSystem;

// TO-DO: MAKE A GOOD DEBUG SYSTEM

public enum EnemyState {Idle,MovingToTask,DoingTask,MovingToInvestigate,Investigating,SpottedPlayer,MovingToMask,FixingMask}
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
    private EnvironmentalMask currentMask, desiredMask;

    public EnemyState state { get; private set; }

    private void Awake()
    {
        
    }

    private void Start()
    {
        ProceedToTask(ChooseRandomTask());
        agent.speed = walkingSpeed;
        AudioManager.Play("TestKings",gameObject,true);
    }
    bool init;
    private void OnEnable()
    {
        if (init) return;

        potentialTasks = Rand.ShuffleCollection(LevelController.Instance.Tasks).ToList();

        init = true;
    }
    Vector3 lastpos;
    public void Update()
    {
        TryChangeState();

        debugText.text = $"State: {state}";
        //Debug stuff
    }
    private void LateUpdate()
    {
        lastpos = transform.position;
    }
    private bool CanSeePlayer()
    {
        Vector3 direction = (Player.Controller.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, direction);
        if(angle <= (FOV / 2f) && Vector3.Distance(Player.Controller.transform.position,transform.position) <= detectionDistance)
        {
            if(Physics.Raycast(transform.position,direction,out RaycastHit hit,99,255,QueryTriggerInteraction.Ignore))
            {
                if(hit.collider.gameObject == Player.Controller.gameObject)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private float losTimeOnPlayer;
    private void TryChangeState()
    {
        switch (state)
        {
            case EnemyState.Idle:
                //Maybe use Idle is a transition state where it wants to see what its next priority should be

                agent.speed = walkingSpeed;

                if (PlayerSensed())
                {
                    break;
                }

                if(SoundDetection.instance.ActiveMasks.Count > 0)
                {
                    
                    GoToMask();
                    break;
                }

                TryResumeTask();

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

                if (SoundDetection.instance.ActiveMasks.Count > 0)
                {
                    
                    GoToMask();
                    break;
                }

                if (Vector3.Distance(transform.position, DesiredTask.transform.position) < 1)
                {
                    DesiredTask.StartTask(this);
                    //agent.ResetPath();
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





            case EnemyState.MovingToMask:
                //if sees player break
                //if gets to task break and call task 
                //if noise is over threshold investigate


                if (PlayerSensed())
                {
                    break;
                }

                if (!desiredMask.isOn)
                {
                    investigationPoint = desiredMask.transform.position;
                    agent.SetDestination(desiredMask.transform.position);
                    state = EnemyState.MovingToInvestigate;
                    break;
                }

                if (SoundDetection.instance.OrderedMasks[0].maskLevel > desiredMask.maskLevel)
                {
                    GoToMask();
                }

                Collider col = desiredMask.GetComponent<Collider>();
                //print(Vector3.Distance(transform.position, col.ClosestPointOnBounds(transform.position)));
                if(Vector3.Distance(transform.position,col.ClosestPointOnBounds(transform.position)) < 1)
                //if (Vector3.Distance(transform.position,desiredMask.transform.position) < 1)
                {
                    desiredMask.SetFixing(true,this);
                    //agent.ResetPath();
                    currentMask = desiredMask;
                    state = EnemyState.FixingMask;
                    break;
                }
                break;

            ///
            /// Doing Task
            ///
            case EnemyState.FixingMask:
                //if sees player break
                //if noise is over threshold investigate
                //get the called task to call complete in here when completed
                //if breaks early from task, call task stopped or something


                if (PlayerSensed())
                {
                    currentMask.SetFixing(false,this);
                    currentMask = null;
                    break;
                }
                if (!desiredMask.isOn)
                {
                    state = EnemyState.Idle;
                    break;
                }
                break;




            case EnemyState.MovingToInvestigate:
                //if sees player break
                //if get to area, start investigating noise itself (looking around and stuff)


                if (PlayerSensed())
                {
                    break;
                }
                //|| agent.remainingDistance <= 0.5f
                if (Vector3.Distance(transform.position,investigationPoint) < 1 || (agent.remainingDistance < 1 && agent.pathStatus == NavMeshPathStatus.PathPartial))
                {
                    state = EnemyState.Investigating;
                    startingRotFromInvestigation = transform.eulerAngles.y;
                    timeOfStartInvestigation = Time.time;
                    break;
                }

                break;

            case EnemyState.Investigating:
                //if sees player break
                //looks around for x seconds then gets back to task if it wasnt complete, or else go to a new task


                if (PlayerSensed())
                {
                    break;
                }

                Vector3 rot = transform.eulerAngles;
                rot.y = startingRotFromInvestigation + (90 * Mathf.Sin(Time.time - timeOfStartInvestigation));

                transform.eulerAngles = rot;

                if (Time.time >= timeOfStartInvestigation + 5f)
                {
                    state = EnemyState.Idle;
                    agent.speed = walkingSpeed;
                }
                break;

            case EnemyState.SpottedPlayer:

                //adds x permanent sus per second when spotted
                //if permanent sus is over y then plays game over cutscene

                transform.LookAt(Player.Controller.transform.position);
                if (SoundDetection.instance.IsTaggedAndCursed)
                {
                    agent.ResetPath();
                    Player.Controller.SetPlayerControl(false);
                    break;
                }
                if (!CanSeePlayer())
                {
                    state = EnemyState.MovingToInvestigate;
                    investigationPoint = Player.Controller.transform.position;
                    agent.SetDestination(Player.Controller.transform.position);
                    break;
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
            losTimeOnPlayer += Time.deltaTime;
            if(losTimeOnPlayer >= 0.25f)
            {
                PlayerSeen();
                agent.speed = walkingSpeed;
                return true;
            }
        }
        else
        {
            losTimeOnPlayer = Mathf.Clamp01(losTimeOnPlayer -= Time.deltaTime);
        }
        
        if (SoundDetection.instance.IsDetected)
        {
            bool navEdge = NavMesh.SamplePosition(Player.Controller.transform.position, out NavMeshHit hit, 0.25f, 1);
            if (navEdge)
            {
                if(hit.distance > 0.1f)
                {
                    
                }
                print("Found nearby nav");
            }
            PlayerHeard();
            agent.speed = investigatingSpeed;
            return true;
        }
        return false;
    }
    private void PlayerSeen()
    {
        transform.LookAt(Player.Controller.transform.position);
        state = EnemyState.SpottedPlayer;
        agent.SetDestination(Player.Controller.transform.position);
    }
    private void PlayerHeard()
    {
        state = EnemyState.MovingToInvestigate;
        investigationPoint = Player.Controller.transform.position;
        agent.SetDestination(Player.Controller.transform.position);
    }

    private void InvestigateArea(Vector3 location)
    {
        investigationPoint = location;
        agent.SetDestination(investigationPoint);
        state = EnemyState.MovingToInvestigate;
    }
    
    public void ProceedToTask(Task task)
    {
        agent.SetDestination(task.transform.position);
        
        DesiredTask = task;
        state = EnemyState.MovingToTask;
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
        state = EnemyState.Idle;
        //ProceedToTask(ChooseRandomTask());
    }
    private void TryResumeTask()
    {
        if(!potentialTasks.Contains(DesiredTask)) ProceedToTask(ChooseRandomTask());
        if (DesiredTask.completed)
        {
            ProceedToTask(ChooseRandomTask());
            return;
        }
        ProceedToTask(DesiredTask);
    }
    Vector3 investigationPoint;

    public void FinishedFixingMask()
    {
        state = EnemyState.Idle;
    }

    public void GoToMask()
    {
        agent.speed = investigatingSpeed;
        desiredMask = SoundDetection.instance.OrderedMasks[0];
        agent.ResetPath();
        agent.SetDestination(desiredMask.transform.position);
        state = EnemyState.MovingToMask;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Door door = collision.gameObject.GetComponent<Door>();
        if (door != null)
        {
            door.OpenDoor(true);
        }
    }
}

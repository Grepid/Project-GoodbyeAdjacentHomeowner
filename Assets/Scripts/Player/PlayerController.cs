using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private CharacterController cc;

    [SerializeField]
    private GameObject camArm;

    public bool controlling { get; private set; }

    public PlayerInputControls PlayerInputControls;
    private InputAction IA_Movement;
    private InputAction IA_Jump;
    private InputAction IA_Look;


    [SerializeField]
    private float moveSpeed;

    private float adjustedSpeed;

    [SerializeField]
    private Vector2 mouseSens;

    private Vector3 velocity;

    private Vector3 movementDirection;
    private Vector2 lookXY;

    [SerializeField]
    private float crouchHeight;
    private float standHeight;

    [SerializeField]
    private float crouchSpeedMultiplier,startingSprintMultiplier;
    public float SprintMultiplier { get; private set; }
    public void SetSprintMultiplier(float newMultiplier)
    {
        newMultiplier = Mathf.Clamp(newMultiplier,1,float.MaxValue);
        SprintMultiplier = newMultiplier;
    }
    public bool Crouched { get; private set; }

    public float PlayerGravity;
    public float JumpStrength;
    public float ExtraJumpStrength = 1.5f;
    bool isCheckingForGround = true;
    public int AllowedJumps;
    private int currentJumps;
    private Vector3 lastPos;

    public float susPerSecond;

    public bool isGrounded { get; private set; }
    public bool canSprint;
    private void Awake()
    {
        Player.SetController(this);
        this.PlayerInputControls = new PlayerInputControls();
        SetSprintMultiplier(startingSprintMultiplier);
        standHeight = cc.height;
        currentJumps = AllowedJumps;
    }

    private void OnEnable()
    {
        IA_Movement = PlayerInputControls.Player.Move;
        IA_Movement.Enable();

        IA_Jump = PlayerInputControls.Player.Jump;
        IA_Jump.Enable();
        IA_Jump.performed += JumpCall;

        IA_Look = PlayerInputControls.Player.Look;
        IA_Look.Enable();
    }
    private void OnDisable()
    {
        IA_Movement.Disable();
        IA_Jump.Disable();
        IA_Look.Disable();
    }

    private void Start()
    {
        SetCursor(false);
    }

    private void FixedUpdate()
    {
        CheckGrounded();
    }


    // Update is called once per frame
    void Update()
    {
        //if (<add bool for player controls here>) return;
        AssignVariables();
        MovementUpdate();
        CameraUpdate();
        CheckInputs();
        if (movementDirection == Vector3.zero) return;
        float sus = susPerSecond;
        //if (Crouched) sus *= crouchSpeedMultiplier;
        if (Crouched) sus *= 0;
        //if (Input.GetKey(KeyCode.LeftShift)) sus *= SprintMultiplier;
        SoundDetection.instance.AddTemporarySuspicionPercent(sus * Time.deltaTime);
    }
    private void LateUpdate()
    {
        lastPos = transform.position;
    }
    private void AssignVariables()
    {
        //Left / Right
        float lookX = 0;//Input.GetAxis("Mouse X") * Time.smoothDeltaTime * mouseSens.x;
        //Up / Down
        float lookY = 0;//Input.GetAxis("Mouse Y") * Time.smoothDeltaTime * mouseSens.y * -1;

        Vector2 look = IA_Look.ReadValue<Vector2>();

        look *= Time.smoothDeltaTime;

        look.x *= mouseSens.x;
        look.y *= mouseSens.y;

        lookXY.x = Mathf.Clamp(lookXY.x + -look.y, -90, 90);
        lookXY.y += look.x;

        //Forward / Backward
        float fb = 0;// Input.GetAxis("Vertical");

        //Left / Right
        float lr = 0;// Input.GetAxis("Horizontal");


        Vector2 movDir = IA_Movement.ReadValue<Vector2>();

        movementDirection = transform.forward * movDir.y + transform.right * movDir.x;

        //Will add a multiplier to speed if the player is holding shift (will be dynamic later)
        adjustedSpeed = 1;// Input.GetKey(KeyCode.LeftShift) && CanSprint() ? moveSpeed * SprintMultiplier : moveSpeed;
        adjustedSpeed = Crouched ? adjustedSpeed * crouchSpeedMultiplier : adjustedSpeed;


    }
    private bool CanSprint()
    {
        bool result = true;
        result &= isGrounded;
        result &= canSprint;
        return result;
    }
    private void CheckGrounded()
    {
        if (!isCheckingForGround) return;

        var hits = Physics.OverlapSphere(transform.position + Vector3.down, 0.25f, 255, QueryTriggerInteraction.Ignore);
        //Counts hits that isn't the player
        int notPlayer = 0;
        foreach (var hit in hits)
        {
            if (hit.gameObject != transform.gameObject) notPlayer++;
        }

        //If the player is touching no objects, they're not on the ground
        if (notPlayer == 0) isGrounded = false;

        //If they touch something on their bottom that is not themselves, they are on the ground
        else
        {
            isGrounded = true;
            //Resets Jump counter
            currentJumps = AllowedJumps;
        }
    }
    private void CameraUpdate()
    {
        camArm.transform.rotation = Quaternion.Euler(new Vector3(lookXY.x, lookXY.y, 0));
        transform.rotation = Quaternion.Euler(new Vector3(0, lookXY.y, 0));
    }
    private void MovementUpdate()
    {
        cc.Move(movementDirection * Time.deltaTime * adjustedSpeed);
        cc.Move(velocity * Time.deltaTime);
        if (isGrounded) velocity = Vector3.down * PlayerGravity;
        else
        {
            velocity += (Vector3.down * PlayerGravity * Time.deltaTime);
        }

        //If player hits their head while its trying to continue jump velocity, cancel Y velocity
        if (velocity.y > 0 && (transform.position.y == lastPos.y))
        {
            velocity.y = 0;
        }
    }
    private void CheckInputs()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.X))
        {
            Crouch(true);
        }
        if (Input.GetKeyUp(KeyCode.LeftControl)||Input.GetKeyUp(KeyCode.X))
        {
            Crouch(false);
        }

        //Debug
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("AnyTest");
        }*/

    }

    private void JumpCall(InputAction.CallbackContext context)
    {
        Jump();
    }
    private void Jump()
    {
        if (currentJumps == 0) return;
        float strength = currentJumps != AllowedJumps ? JumpStrength * ExtraJumpStrength : JumpStrength;
        currentJumps--;
        velocity = Vector3.up * strength;
        SoundDetection.instance.AddTemporarySuspicionPercent(10);

        StartPostJump();
    }
    /// <summary>
    /// Begins the process of stopping ground checks, and checking for when to allow them again
    /// </summary>
    private void StartPostJump()
    {
        isGrounded = false;
        isCheckingForGround = false;
        StopCoroutine(CheckForApex());
        StartCoroutine(CheckForApex());
    }

    /// <summary>
    /// Checks for when the player is no longer going up
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckForApex()
    {
        while (true)
        {
            if (velocity.y < 0)
            {
                isCheckingForGround = true;
                break;
            }
            yield return null;
        }

    }

    private IEnumerator WaitForUncrouch()
    {
        while (true)
        {
            Vector3 footPos = cc.bounds.center - Vector3.up * cc.bounds.extents.y;
            var hits = Physics.RaycastAll(footPos, Vector3.up, standHeight);
            var nonPlayer = Array.Find(hits, h => h.collider.gameObject != gameObject);
            if (nonPlayer.collider != null)
            {
                
            }
            else
            {
                cc.height = standHeight;
                Crouched = false;
                break;
            }
            yield return null;
        }
    }
    

    private void Crouch(bool value)
    {
        if (!value)
        {
            StopCoroutine(WaitForUncrouch());
            StartCoroutine(WaitForUncrouch());
        }
        else
        {
            cc.height = crouchHeight;
            Crouched = true;
        }
    }

    public void SetCursor(bool value)
    {
        Cursor.lockState = value ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = value;
        SetPlayerControl(!value);
    }
    public void SetPlayerControl(bool value)
    {
        controlling = value;
    }

}


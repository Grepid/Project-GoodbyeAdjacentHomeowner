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

    public PlayerInputControls PIC;
    private InputAction IA_Movement, IA_Look;


    [SerializeField]
    private float moveSpeed;

    private float adjustedSpeed;

    [SerializeField]
    private Vector2 mouseSens,gamepadSens;

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
    public PlayerInput PlayerIn;
    private void Awake()
    {
        Player.SetController(this);
        this.PIC = new PlayerInputControls();
        SetSprintMultiplier(startingSprintMultiplier);
        standHeight = cc.height;
        currentJumps = AllowedJumps;
    }
    private void OnEnable()
    {
        PIC.Enable();

        IA_Movement = PIC.Player.Move;

        IA_Look = PIC.Player.Look;
    }
    private void OnDisable()
    {
        PIC.Disable();
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
        if (!controlling) return;
        AssignVariables();
        MovementUpdate();
        CheckInputs();
        if (movementDirection == Vector3.zero) return;
        float sus = susPerSecond;
        //if (Crouched) sus *= crouchSpeedMultiplier;
        if (Crouched) sus *= 0;
        if (PIC.Player.Sprint.IsPressed()) sus *= SprintMultiplier;
        SoundDetection.instance?.AddTemporarySuspicionPercent(sus * IA_Movement.ReadValue<Vector2>().magnitude * Time.deltaTime);
    }
    private void LateUpdate()
    {
        CameraUpdate();
        lastPos = transform.position;
    }
    private void AssignVariables()
    {
        Vector2 look = IA_Look.ReadValue<Vector2>();

        look *= Time.smoothDeltaTime;
        //look *= Time.deltaTime;

        look.x *= PlayerIn.currentControlScheme == PIC.KeyboardMouseScheme.name? mouseSens.x : gamepadSens.x;
        look.y *= PlayerIn.currentControlScheme == PIC.KeyboardMouseScheme.name ? mouseSens.y : gamepadSens.y;

        //look *= (Gamepad.current != null && Gamepad.current.rightStick.magnitude > 0) ? 20 : 1;

        

        lookXY.x = Mathf.Clamp(lookXY.x + -look.y, -90, 90);
        lookXY.y += look.x;

        Vector2 movDir = IA_Movement.ReadValue<Vector2>();

        movementDirection = transform.forward * movDir.y + transform.right * movDir.x;

        //Will add a multiplier to speed if the player is holding shift (will be dynamic later)
        adjustedSpeed = PIC.Player.Sprint.IsPressed() && CanSprint() ? moveSpeed * SprintMultiplier : moveSpeed;

        

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
    public bool crouchToggle;
    private void CheckInputs()
    {
        
        if (PIC.Player.Crouch.WasPressedThisFrame())
        {
            if(crouchToggle && Crouched)
            {
                Crouch(false);
                return;
            }
            Crouch(true);
        }
        if (PIC.Player.Crouch.WasReleasedThisFrame())
        {
            if (crouchToggle) return;
            Crouch(false);
        }
        

        //Debug
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene("AnyTest");
        }
        
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            print($"Current Ambience set to: {SoundDetection.instance.AmbienceLevel}");
        }
    }

    public void OnInteract()
    {
        TryInteract();
    }
    private void TryInteract()
    {
        if (InteractionSystem.s_lastHit.DidHit)
        {
            InteractionBase i = InteractionSystem.s_lastHit.Interactable;
            if (i != null)
            {
                i.TryInteract();
                WindowTraversal win = i.GetComponent<WindowTraversal>();
                window = win;
            }
        }
    }
    private WindowTraversal window;

    private void OnJump()
    {
        Jump();
    }
    private void Jump()
    {
        if (currentJumps == 0) return;
        float strength = currentJumps != AllowedJumps ? JumpStrength * ExtraJumpStrength : JumpStrength;
        currentJumps--;
        velocity = Vector3.up * strength;

        SoundDetection.instance?.AddTemporarySuspicionPercent(10);

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

    public void OnTraverse()
    {
        if(window != null)
        {
            window.Traverse();
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
        if (value) PIC.Enable();
        else PIC.Disable();
    }
    public void TPPlayer(Vector3 pos)
    {
        cc.enabled = false;
        transform.position = pos;
        cc.enabled = true;
    }
}


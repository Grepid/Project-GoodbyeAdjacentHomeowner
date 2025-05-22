using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Adjacent;
using Unity.Netcode;
using QFSW.QC;

public class PlayerController : NetworkBehaviour
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
    public Camera playerCam;
    public Vector3 center => transform.position + cc.center;
    /*private void Awake()
    {
        if (!IsOwner)
        {
            playerCam.gameObject.SetActive(false);
            //return;
        }

        Player.SetController(this);
        PIC = new PlayerInputControls();
        PIC.Enable();
        SetSprintMultiplier(startingSprintMultiplier);
        standHeight = cc.height;
        currentJumps = AllowedJumps;
    }*/
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner)
        {
            playerCam.gameObject.SetActive(false);
            return;
        }

        Player.SetController(this);
        PIC = new PlayerInputControls();
        PIC.Enable();
        SetSprintMultiplier(startingSprintMultiplier);
        standHeight = cc.height;
        currentJumps = AllowedJumps;
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

    }

    [Command]
    private void DescribeRelations()
    {
        if (IsOwner)
        {
            print($"Function ran by Owner {base.NetworkObject.OwnerClientId}");
        }
        else
        {
            print($"Function failed to run from non-owner {base.NetworkObject.OwnerClientId}");
        }
    }


    /*private void OnEnable()
    {
        if (IsOwner)
        {
            print($"Awake ran on Owner {base.NetworkObject.OwnerClientId}");
        }
        else
        {
            print($"Awake ran and failed on non-owner {base.NetworkObject.OwnerClientId}");
        }

        PIC.Enable();
    }*/
    private void OnDisable()
    {
        if (!IsOwner) return;
        PIC.Disable();
    }

    private void Start()
    {
        if (!IsOwner) return;
        SetCursor(false);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        CheckGrounded();
    }
    

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (!controlling) return;
        AssignVariables();
        MovementUpdate();
        CheckInputs();
        if (movementDirection == Vector3.zero) return;
        float sus = susPerSecond;
        //if (Crouched) sus *= crouchSpeedMultiplier;
        if (Crouched) sus *= 0;
        if (PIC.Player.Sprint.IsPressed()) sus *= SprintMultiplier;
        SoundDetection.instance?.AddTemporarySuspicionPercent(sus * PIC.Player.Move.ReadValue<Vector2>().magnitude * Time.deltaTime);
    }
    private void LateUpdate()
    {
        if (!IsOwner) return;
        CameraUpdate();
        lastPos = transform.position;
    }
    private void AssignVariables()
    {
        if (!IsOwner) return;
        Vector2 look = PIC.Player.Look.ReadValue<Vector2>();

        look *= Time.smoothDeltaTime;
        //look *= Time.deltaTime;

        look.x *= PlayerIn.currentControlScheme == PIC.KeyboardMouseScheme.name? mouseSens.x : gamepadSens.x;
        look.y *= PlayerIn.currentControlScheme == PIC.KeyboardMouseScheme.name ? mouseSens.y : gamepadSens.y;

        //look *= (Gamepad.current != null && Gamepad.current.rightStick.magnitude > 0) ? 20 : 1;

        

        lookXY.x = Mathf.Clamp(lookXY.x + -look.y, -90, 90);
        lookXY.y += look.x;

        Vector2 movDir = PIC.Player.Move.ReadValue<Vector2>();

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

        var hits = Physics.OverlapSphere(transform.position, 0.1f, 255, QueryTriggerInteraction.Ignore);
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
        if (!IsOwner) return;
        camArm.transform.rotation = Quaternion.Euler(new Vector3(lookXY.x, lookXY.y, 0));
        transform.rotation = Quaternion.Euler(new Vector3(0, lookXY.y, 0));
    }
    private void MovementUpdate()
    {
        if (!IsOwner) return;
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
        if (!IsOwner) return;
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
        if (PIC.Player.Jump.IsPressed())
        {
            Jump();
        }
        

        //Debug
        /*
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene("AnyTest");
        }
        
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            print($"Current Ambience set to: {SoundDetection.instance.AmbienceLevel}");
        }*/
    }

    public void OnInteract()
    {
        if (!IsOwner) return;
        TryInteract();
    }
    private void TryInteract()
    {
        if (!IsOwner) return;
        //If Interaction system hit an object
        if (InteractionSystem.s_lastHit.collider != null)
        {
            var h = InteractionSystem.s_lastHit.collider.gameObject;

            //Grab then try interact with an interactable
            InteractionBase i = InteractionSystem.s_lastHit.collider.GetComponent<InteractionBase>();
            if (i != null)
            {
                i.TryInteract();
            }
            else
            {
                //Checks if the hit object is a Window Traversal Zone to register it
                WindowTraversal win = h.GetComponent<WindowTraversal>();
                window = win;
            }
        }
        else
        {
            window = null;
        }
        
    }
    private WindowTraversal window;

    private void OnJump()
    {
        if (!IsOwner) return;
        Jump();
    }
    private void Jump()
    {
        if (!IsOwner) return;
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
        if (!IsOwner) return;
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
        if (!IsOwner) yield break;
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
        if (!IsOwner) return;
        if (window != null)
        {
            window.Traverse();
        }
    }

    private IEnumerator WaitForUncrouch()
    {
        if (!IsOwner) yield break;
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
        if (!IsOwner) return;
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
        if (!IsOwner) return;
        Cursor.lockState = value ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = value;
        SetPlayerControl(!value);
    }
    public void SetPlayerControl(bool value)
    {
        if (!IsOwner) return;
        controlling = value;
        if (value) PIC.Enable();
        else PIC.Disable();
    }
    public void TPPlayer(Vector3 pos)
    {
        if (!IsOwner) return;
        cc.enabled = false;
        transform.position = pos;
        cc.enabled = true;
    }
}


using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Horizontal movement")]
    [SerializeField] private float walkSpeed = 8f;
    [HideInInspector] public float speed;
    [SerializeField] private float sprintMultiplier = 2f;
    [HideInInspector] public float maxSpeed;
    [SerializeField] private float airMultiplier = 0.8f;
    [SerializeField] private float rotationSpeed = 10f;
    private float speedMultiplier = 1f;
    private bool sprint;    

    private Vector3 moveDirection;
    private Vector3 moveVelocity;
   

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 35f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCooldown = 0.6f;
    private float timeToDash;
    private IEnumerator dashCoroutine;

    [Header("Jump")]
    [SerializeField] private float gravity = -70f;
    [SerializeField] private float initialJumpForce = 8f;
    [SerializeField] private float jumpCooldown = 0.3f;
    [HideInInspector] public float verticalVelocity;
    private float terminalVelocity = -60f; //Max fall speed

    [SerializeField] private float groundRaycastDistance = 0.2f;
    
    [HideInInspector] public bool jumped;
    [HideInInspector] public bool canJump;

    [Header("Double jump")]
    [SerializeField] private int numberOfJumps = 3;
    [HideInInspector] private float jumpForce;
    [HideInInspector] public int remainingJumps;
    [SerializeField] private bool dampNextJump = true;
    [SerializeField] private float dampJumpFactor = 1.4f;
    [HideInInspector] public int jumpCount;
    private float timeToJump;


    [Header("Wall jump")]
    [SerializeField] private float wallGravity = -4.0f;
    [SerializeField] private float wallRaycastDistance = 0.6f;
    [SerializeField] private float wallJumpMultiplier = 2f;
    [SerializeField] private LayerMask wallLayer;
    
    private Vector3 wallJumpDirection;


    [Header("Slopes")]
    [SerializeField] private float slopeRaycastDistance = 0.55f;
    private bool exitingSlope;
    
    private RaycastHit slopeHit;


    [HideInInspector] public bool isSprinting;
    [HideInInspector] public bool isWalking;
    [HideInInspector] public bool isMoving;
    [HideInInspector] public bool isDashing;

    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool onSlope;
    [HideInInspector] public bool onWall;
    [HideInInspector] public bool wallOnRight, wallOnLeft, wallOnFront;

    [Space(10)]
    [SerializeField] public bool isLockedOn;
    [SerializeField] public Transform lockTarget;


    [Space(10)]
    [Header("SFX")]
    [SerializeField] private AudioEventSO jumpSFX;
    [SerializeField] private AudioEventSO tripleJumpSFX;
    [SerializeField] private AudioEventSO dashSFX;

    [Header("VFX")]
    [SerializeField] private ParticleSystem jumpVFX;
    [SerializeField] private MeshTrailVFX dashVFX;

    [HideInInspector] public CharacterController playerController;
    private Transform mainCamera;
    private PlayerInput playerInput;

    public static PlayerMovement Instance;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        GetComponents();
        InitializeValues();
    }


    private void InitializeValues()
    {
        remainingJumps = numberOfJumps;
        timeToJump = Time.time;
        timeToDash = Time.time;

        maxSpeed = walkSpeed * sprintMultiplier;

        if (jumpVFX != null)
        {
            jumpVFX = Instantiate(jumpVFX, transform);
            jumpVFX.Stop();
        }

        if (dashVFX != null)
        {
            dashVFX = Instantiate(dashVFX, transform);
            dashVFX.trailDuration = dashTime;
        }
    }


    private void GetComponents()
    {
        playerController = GetComponent<CharacterController>();
        mainCamera = Camera.main.transform;
        playerInput = GetComponent<PlayerInput>();
    }


    // Update is called once per frame
    void Update()
    {
        CheckIsGrounded();
        CheckOnSlope();
        CheckOnWall();

        Gravity();
        Jump();
        Move();
    }

    private void LateUpdate()
    {
        UpdateJumpCount();


    }





    private void OnMove(InputValue inputValue)
    {
        Vector2 inputDirection = inputValue.Get<Vector2>();

        //Cast input direction into a 3D vector
        moveDirection = new Vector3(inputDirection.x, 0.0f, inputDirection.y);

        //Rotate the vector with respect to the camera
        moveDirection = Quaternion.Euler(0f, mainCamera.eulerAngles.y, 0f) * moveDirection;

        //Project onto slopes
        if (onSlope && !exitingSlope) moveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);

        moveDirection = moveDirection.normalized;
        //Debug.Log("Input direction = " + moveDirection);
    }


    private void Move()
    {
        Vector3 moveAmount = Vector3.zero;
        isMoving = (moveDirection != Vector3.zero);

        if (!isMoving)
        {
            isWalking = isSprinting = false;
            speed = 0f;

        }
        else
        {
            isSprinting = sprint;
            isWalking = !isSprinting;

            speedMultiplier = sprint ? sprintMultiplier : 1;
            float airControl = isGrounded ? 1 : airMultiplier;
            speed = walkSpeed * speedMultiplier * airControl;

            //Velocity
            Vector3 moveVelocity = moveDirection * speed;
            Vector3 horizontalMoveAmount = moveVelocity * Time.deltaTime;
            moveAmount += horizontalMoveAmount;

            FaceCameraForward();

        }

        

        //Vertical
        Vector3 verticalMoveAmount = new Vector3(0, verticalVelocity, 0) * Time.deltaTime;
        moveAmount += verticalMoveAmount;

        playerController.Move(moveAmount);

        //if (isLockedOn && lockTarget != null) LookAtTarget();

    }

    private void LookAtTarget()
    {
        Vector3 targetDirection = lockTarget.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

    }
    


    private void Gravity()
    {
        if (isGrounded && verticalVelocity < 0) verticalVelocity = -2.0f;
        //if (onWall && verticalVelocity < 0) verticalVelocity = -0.0f; //Nullify gravity on wall

        //Apply gravity
        if (verticalVelocity > terminalVelocity)
        {
            if (onWall)
            {
                verticalVelocity += wallGravity * Time.deltaTime;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
        }
    }

    private void OnJump()
    {
        jumped = true;
    }

    private void Jump()
    {
        canJump = (remainingJumps > 0 && Time.time > timeToJump);

        if (jumped && canJump)
        {
            exitingSlope = true;

            if (jumpSFX != null) jumpSFX.Play();
            if (jumpCount == numberOfJumps)
            {
                if (jumpVFX != null) jumpVFX.Play();
                if (tripleJumpSFX != null) tripleJumpSFX.Play();
            }


            if (onWall)
            {
                //Debug.Log("WALL JUMP!");
                jumpVFX.Play();
                StartCoroutine(WallJump(0.2f));
            }


            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
            if (dampNextJump) jumpForce = jumpForce * dampJumpFactor;

            remainingJumps--;

            timeToJump = Time.time + jumpCooldown;
        }
    }


    private IEnumerator WallJump(float walljumpDuration)
    {
        float walljumpEnd = Time.time + walljumpDuration;

        while (Time.time < walljumpEnd)
        {
            playerController.Move(wallJumpDirection * jumpForce * wallJumpMultiplier * Time.deltaTime);
            yield return null;
        }

        //transform.forward = wallJumpDirection;
    }


    private void OnSprint(InputValue inputValue)
    {
        //Debug.Log("Sprint");
        sprint = inputValue.isPressed;
    }



    private void OnDash()
    {
        //Debug.Log("Dash");
        if (Time.time > timeToDash)
        {
            if (dashCoroutine != null)
            {
                StopCoroutine(dashCoroutine);
            }

            dashCoroutine = Dash(dashTime, dashSpeed);
            StartCoroutine(dashCoroutine);

            timeToDash = Time.time + dashCooldown;
        }
    }


    IEnumerator Dash(float dashTime, float dashSpeed)
    {
        float startTime = Time.time;
        exitingSlope = true;


        if (dashSFX != null) dashSFX.Play();
        if (dashVFX != null) dashVFX.Play();

        isDashing = true;
        while (Time.time < startTime + dashTime /*&& Time.timeScale != 0*/) //the timeScale != 0 breaks the coroutine, so all momentum is lost
        {
            playerController.Move(transform.forward * dashSpeed * Time.deltaTime);
            yield return new WaitForSeconds(Time.deltaTime);
            //yield return null; //Does not freeze with timeScale
        }
        isDashing = false;

    }

    private void FaceCameraForward()
    {
        //Rotates the player to the camera forward when we try to move forward

        float targetAngle;

        //targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
        //targetAngle = mainCamera.eulerAngles.y;
        targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

        Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }


    private void ResetJumps()
    {
        remainingJumps = numberOfJumps;
        jumpForce = initialJumpForce;
        exitingSlope = false;
    }

    private void UpdateJumpCount()
    {
        if (jumped) jumped = false;
        jumpCount = numberOfJumps - remainingJumps + 1;
    }

    //CHECKS: OnWall, Grounded, OnSlope

    private void CheckIsGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundRaycastDistance);

        //isGrounded = playerController.isGrounded;

        if ((isGrounded || onWall) && Time.time > timeToJump) ResetJumps();
    }


    private void CheckOnWall()
    {
        Vector3 rightDirection = transform.right * wallRaycastDistance + transform.position + Vector3.up * 0.5f;
        Vector3 leftDirection = -transform.right * wallRaycastDistance + transform.position + Vector3.up * 0.5f;

        Debug.DrawLine(transform.position + Vector3.up * 0.5f, rightDirection, Color.yellow);
        Debug.DrawLine(transform.position + Vector3.up * 0.5f, leftDirection, Color.blue);

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.right, wallRaycastDistance, wallLayer))
        {
            //Debug.Log("Wall on right");
            wallOnRight = true;
            onWall = true;

            wallJumpDirection = -transform.right;
        }

        else if (Physics.Raycast(transform.position + Vector3.up * 0.5f, -transform.right, wallRaycastDistance, wallLayer))
        {
            //Debug.Log("Wall on left");
            wallOnLeft = true;
            onWall = true;

            wallJumpDirection = transform.right;
        }

        else if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, wallRaycastDistance, wallLayer))
        {
            //Debug.Log("Wall in front");
            wallOnFront = true;
            onWall = true;

            wallJumpDirection = -transform.forward;
        }
        else
        {
            onWall = false;
            wallOnLeft = false;
            wallOnRight = false;
            wallOnFront = false;
        }
    }



    private void CheckOnSlope()
    {
        if (Physics.Raycast(transform.position + 0.1f * Vector3.up, Vector3.down, out slopeHit, slopeRaycastDistance))
        {
            if (slopeHit.normal != Vector3.up)
            {
                onSlope = true;
            }
            else
            {
                onSlope = false;
            }
        }
        else
        {
            onSlope = false;
        }
    }


    private void CommentedSections()
    {
        /*
            //Climb walls
            if (wallOnFront)
            {
                Debug.Log("Climb up the walls");
                verticalVelocity = climbSpeed * speedMultiplier;
            }

            //Wall run
            if ((wallOnLeft || wallOnRight) && isSprinting)
            {
                Debug.Log("Wallrunning");
                playerController.Move(transform.forward * wallRunSpeed * speedMultiplier * Time.deltaTime);

            }
            */
    }















}

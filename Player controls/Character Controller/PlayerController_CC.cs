using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CharacterController))]

public class PlayerController_CC : MonoBehaviour
{ 

    [Header("Input")]
    public bool movementEnabled = true;
    public bool isMoving;

    [Header("Horizontal movement")]
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] public float speed;
    [SerializeField] private float sprintMultiplier = 2f;
    [HideInInspector] public float maxSpeed;
    [SerializeField] private float airMultiplier = 0.8f;

    private float speedMultiplier = 1f;
    private bool sprint;
    [SerializeField] public bool isSprinting;
    [SerializeField] public bool isWalking;
    private Vector3 moveDirection;
    private Vector3 moveVelocity;



    [Header("Player-to-camera Rotation")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Dash")]
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashCooldown = 0.6f;
    [SerializeField] public bool isDashing;

    private float timeToDash;
    private IEnumerator dashCoroutine;

    [Header("Jump")]
    [SerializeField] private float gravity = -30f;
    public float verticalVelocity;
    [SerializeField] private float maxJumpForce = 30f;
    [SerializeField] private float jumpCooldown = 0.3f;
    
    [SerializeField] private float groundRaycastDistance = 0.2f;
    [SerializeField] public bool isGrounded;
    [HideInInspector] public bool jumped;
    [HideInInspector] public bool canJump;

    [Header("Double jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private int numberOfJumps = 3;
    [SerializeField] public int remainingJumps;
    [SerializeField] private bool dampNextJump = true;
    [SerializeField] private float dampJumpFactor = 1.4f;
    private float timeToJump;
    [HideInInspector] public int jumpCount;


    [Header("Wall jump")]
    [SerializeField] private Vector3 wallJumpDirection;
    [SerializeField] private float wallGravity = -4.0f;
    [SerializeField] private float wallRaycastDistance = 0.6f;
    [SerializeField] private float wallJumpMultiplier = 2f;
    [SerializeField] public bool onWall;

    [SerializeField] public bool wallOnRight, wallOnLeft, wallOnFront;


    [Header("Wall climb")]
    [SerializeField] private float climbSpeed = 6f;
    [SerializeField] private float wallRunSpeed = 10f;




    [Header("Slopes")]
    [SerializeField] public bool onSlope;
    [SerializeField] private float slopeRaycastDistance = 0.55f;
    [SerializeField] private bool exitingSlope;
    private RaycastHit slopeHit;


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

    public static PlayerController_CC Instance;

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
        jumped = false;
        jumpCount = numberOfJumps - remainingJumps + 1;

    }


    private void OnMove(InputValue inputValue)
    {
        Vector2 inputDirection = inputValue.Get<Vector2>();

        //Cast input direction into a 3D vector
        moveDirection = new Vector3(inputDirection.x, 0.0f, inputDirection.y);

        //Rotate the vector with respect to the camera
        moveDirection = Quaternion.Euler(0f, mainCamera.eulerAngles.y, 0f) * moveDirection;
        moveDirection = moveDirection.normalized;

        //Project onto slopes
        if (onSlope && !exitingSlope) moveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);

        //Debug.Log("Input direction = " + moveDirection);
    }



    private void Move()
    {
        isMoving = (moveDirection != Vector3.zero);
        //bool isMoving =(rb.velocity.magnitude != 0); //What I want is if the player is input movement.

        if (!isMoving)
        {
            speed = 0;
            isWalking = false;
            isSprinting = false;
            return;

        }
        else
        {
            //Check if player is walking or sprinting
            speedMultiplier = sprint ? sprintMultiplier : 1;

            isSprinting = sprint;
            isWalking = !isSprinting;

            //Velocity
            speed = walkSpeed * speedMultiplier;
            Vector3 moveVelocity = moveDirection * speed;
            Vector3 moveAmount = moveVelocity * Time.deltaTime;

            //Debug.Log("VELOCITY: " + moveVelocity);

            //Ground movement
            if (isGrounded)
            {
                playerController.Move(moveAmount);
            }
            //Air
            if (!isGrounded)
            {
                playerController.Move(moveAmount * airMultiplier);
            }

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

            if (moveDirection != Vector3.zero) FaceCameraForward();

        }
    }


    private void Climb()
    {
        if (wallOnFront && isMoving)
        {
            Debug.Log("Climb up the wall");
            verticalVelocity = climbSpeed;
        }
    }


    private void OnJump()
    {
        jumped = true;
    }


    private void Gravity()
    {

        if (isGrounded && verticalVelocity < 0) verticalVelocity = -2.0f;
        //if (onWall && verticalVelocity < 0) verticalVelocity = -0.0f; //Nullify gravity on wall

        //Apply gravity
        if (verticalVelocity > -100f)
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

    private void Jump()
    {
        canJump = (remainingJumps > 0 && Time.time > timeToJump);        

        if (jumped && canJump)
        {
            exitingSlope = true;

            if (jumpSFX != null) jumpSFX.Play();
            if (jumpCount == 3)
            {
                if (jumpVFX != null) jumpVFX.Play();
                if (tripleJumpSFX != null) tripleJumpSFX.Play();
            }


            if (onWall)
            {
                Debug.Log("WALL JUMP!");
                StartCoroutine(WallJump(0.2f));
            }


            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
            if (dampNextJump) jumpForce = jumpForce * dampJumpFactor;

            remainingJumps--;

            timeToJump = Time.time + jumpCooldown;
        }

        

        //Calculate vertical movement
        Vector3 verticalMoveAmount = new Vector3(0, verticalVelocity, 0) * Time.deltaTime;
        playerController.Move(verticalMoveAmount);

        


    }


    private IEnumerator WallJump(float walljumpDuration)
    {
        float walljumpEnd = Time.time + walljumpDuration;

        while(Time.time < walljumpEnd)
        {
            playerController.Move(wallJumpDirection * jumpForce * wallJumpMultiplier *  Time.deltaTime);
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
        jumpForce = maxJumpForce;
        exitingSlope = false;
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

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.right, wallRaycastDistance))
        {
            //Debug.Log("Wall on right");
            wallOnRight = true;
            onWall = true;

            wallJumpDirection = -transform.right;
        }

        else if (Physics.Raycast(transform.position + Vector3.up * 0.5f, -transform.right, wallRaycastDistance))
        {
            //Debug.Log("Wall on left");
            wallOnLeft = true;
            onWall = true;

            wallJumpDirection = transform.right;
        }

        else if(Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, wallRaycastDistance))
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















}

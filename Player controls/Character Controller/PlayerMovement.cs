using Cinemachine;
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
    public float verticalInput;
    public float horizontalInput;
    private float speedMultiplier = 1f;
    private float inputMultiplier = 1f;
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
    [SerializeField] private float wallGravity = -20.0f;
    [SerializeField] private float wallRaycastDistance = 0.6f;
    [SerializeField] private float wallJumpMultiplier = 2f;
    [SerializeField] private LayerMask wallLayer;
        
    private Vector3 wallNormal;

    [Header("Wallrun")]
    [SerializeField] private float wallrunBaseSpeed = 10f;
    [SerializeField] private bool wallrunOnSprint = true;
    [SerializeField] private float wallrunFOV = 70f;
    [SerializeField] private float fovChangeRate = 5f;
    [SerializeField] private float cameraDutchAngle = 30f;


    [Header("Slopes")]
    [SerializeField] private float slopeRaycastDistance = 0.55f;
    private bool exitingSlope;
    
    private RaycastHit slopeHit;


    [HideInInspector] public bool isSprinting;
    [HideInInspector] public bool isWalking;
    [HideInInspector] public bool isMoving;
    [HideInInspector] public bool isDashing;
    [HideInInspector] public bool isWallrunning;
    [HideInInspector] public bool isGrounded = false;
    [HideInInspector] public bool onSlope = false;
    [HideInInspector] public bool onWall = false;
    public bool onRightWall, onLeftWall, onFrontWall;

    [Space(10)]
    [SerializeField] public bool isLockedOn;
    [SerializeField] public Transform lockTarget;


    [Space(10)]
    [Header("SFX")]
    [SerializeField] private AudioEventSO jumpSFX;
    [SerializeField] private AudioEventSO tripleJumpSFX;
    [SerializeField] private AudioEventSO dashSFX;
    [SerializeField] private AudioEventSO footstepsSFX;
    private float timeBetweenFootsteps = 0.28f;
    private float nextFootstepTime;

    [Header("VFX")]
    [SerializeField] private ParticleSystem jumpVFX;
    [SerializeField] private MeshTrailVFX dashVFX;
    [SerializeField] private MeshTrailVFX wallrunVFX;

    [Space(10)]
    [SerializeField] private CinemachineFreeLook freelookCamera;
    private float normalFOV;

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
        if (wallrunVFX != null) wallrunVFX = Instantiate(wallrunVFX, transform);
    }


    private void GetComponents()
    {
        playerController = GetComponent<CharacterController>();
        mainCamera = Camera.main.transform;
        playerInput = GetComponent<PlayerInput>();

        freelookCamera = mainCamera.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineFreeLook;
        normalFOV = freelookCamera.m_Lens.FieldOfView;
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
        Vector2 inputDirection = inputValue.Get<Vector2>().normalized;

        inputMultiplier = inputValue.Get<Vector2>().magnitude;

        //Debug.Log("Magnitude of input vector: " + inputValue.Get<Vector2>().magnitude);

        horizontalInput = inputDirection.x;
        verticalInput = inputDirection.y;

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
            isWalking = isSprinting = isWallrunning = false;
            speed = 0f;

        }
        else
        {
            isSprinting = sprint;
            isWalking = !isSprinting;

            Vector3 moveVelocity;
            speedMultiplier = sprint ? sprintMultiplier : 1;

            //Wallrunning
            if (onWall && (!wallrunOnSprint || isSprinting))
            {
                moveVelocity = WallrunVelocity();                
            }
            //Ground/Air movement
            else
            {
                moveVelocity = GroundAirVelocity();
            }

            Vector3 horizontalMoveAmount = moveVelocity * Time.deltaTime;
            moveAmount += horizontalMoveAmount;


            //Footstep SFX
            if (footstepsSFX && isGrounded && !isDashing && Time.time > nextFootstepTime)
            {
                //Debug.Log("Next footstep sfx " + (timeBetweenFootsteps / speedMultiplier));
                footstepsSFX.Play();
                nextFootstepTime = Time.time + (timeBetweenFootsteps/speedMultiplier);

            }

            if(!isLockedOn && !isWallrunning) FaceCameraForward();

        }        

        //Apply vertical velocity computed in Jump() and Gravity()
        Vector3 verticalMoveAmount = new Vector3(0, verticalVelocity, 0) * Time.deltaTime;
        moveAmount += verticalMoveAmount;


        //Finally: move the character controller
        playerController.Move(moveAmount);

        if (isLockedOn && lockTarget != null) LookAtTarget();
        else ResetLookAtTarget();

    }


    private Vector3 GroundAirVelocity()
    {
        //Calculate the movement velocity when not wallrunning, either grounded or airborne
        Vector3 moveVelocity;
        isWallrunning = false;

        //Calculate speed multiplier based on sprinting and airborne
        float airControl = isGrounded ? 1 : airMultiplier;
        speed = walkSpeed * speedMultiplier * airControl; // * inputMultiplier;

        //Velocity
        moveVelocity = moveDirection * speed;
        //Debug.Log("Move velocity: " + moveVelocity);

        //Reset wallrun camera effects
        if (wallrunVFX) wallrunVFX.Stop();
        if (freelookCamera != null)
        {
            //FOV
            float currentFOV = freelookCamera.m_Lens.FieldOfView;
            if (currentFOV != normalFOV) freelookCamera.m_Lens.FieldOfView = Mathf.Lerp(currentFOV, normalFOV, fovChangeRate * Time.deltaTime);

            //Recenter disabled
            freelookCamera.m_RecenterToTargetHeading.m_enabled = false;
            freelookCamera.m_YAxisRecentering.m_enabled = false;

            //Dutch angle
            float currentAngle = freelookCamera.m_Lens.Dutch;
            freelookCamera.m_Lens.Dutch = Mathf.Lerp(currentAngle, 0f, fovChangeRate * Time.deltaTime);
        }

        return moveVelocity;
    }


    private Vector3 WallrunVelocity()
    {
        //Calculate the movement velocity when wall running and enable visual feedback
        Vector3 moveVelocity;
        isWallrunning = true;

        int polarity; //This is to counteract the fact that the cross-product between a onRightWall normal and Vector3.up points backwards.
        polarity = onRightWall ? -1 : 1;

        // Wallrun direction as the parallel to the wall surface direction
        Vector3 wallrunDirection = Vector3.Cross(wallNormal, Vector3.up) * polarity;

        //Make the transform.forward align to the wallrun direction
        transform.forward = Vector3.Lerp(transform.forward, wallrunDirection, rotationSpeed * Time.deltaTime);

        //Calculate velocity
        moveVelocity = wallrunDirection * wallrunBaseSpeed * speedMultiplier;


        //Visuals
        if (wallrunVFX != null) wallrunVFX.Play();
        if (freelookCamera != null)
        {
            //FOV
            float currentFOV = freelookCamera.m_Lens.FieldOfView;
            freelookCamera.m_Lens.FieldOfView = Mathf.Lerp(currentFOV, wallrunFOV, fovChangeRate * Time.deltaTime);

            //Center to target
            freelookCamera.m_RecenterToTargetHeading.m_enabled = true;
            freelookCamera.m_YAxisRecentering.m_enabled = true;
            freelookCamera.m_YAxisRecentering.m_RecenteringTime = 0.2f;
            freelookCamera.m_RecenterToTargetHeading.m_RecenteringTime = 0.2f;

            //Dutch angle
            float currentAngle = freelookCamera.m_Lens.Dutch;
            freelookCamera.m_Lens.Dutch = Mathf.Lerp(currentAngle, -polarity * cameraDutchAngle, fovChangeRate * Time.deltaTime);
        }

        return moveVelocity;
    }


    private void LookAtTarget()
    {
        Vector3 targetDirection = lockTarget.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed / 2f);

    }
    
    public void ResetLookAtTarget()
    {
        //Debug.Log("Transform rotation " + transform.rotation);
        float newXrotation = Mathf.Lerp(transform.rotation.x, 0f, rotationSpeed * Time.deltaTime);
        float newZrotation = Mathf.Lerp(transform.rotation.z, 0f, rotationSpeed * Time.deltaTime);

        Quaternion rotation = new Quaternion(newXrotation, transform.rotation.y, newZrotation, transform.rotation.w);

        transform.rotation = rotation;
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
            playerController.Move(wallNormal * jumpForce * wallJumpMultiplier * Time.deltaTime);
            yield return null;
        }

        //transform.forward = wallNormal;
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


        Vector3 dashDirection;

        if (moveDirection == Vector3.zero)
        {
            dashDirection = transform.forward;
        }
        else
        {
            dashDirection = moveDirection;
        }

        //Debug.Log("Dash direction = " + dashDirection);

        isDashing = true;
        while (Time.time < startTime + dashTime /*&& Time.timeScale != 0*/) //the timeScale != 0 breaks the coroutine, so all momentum is lost
        {
            playerController.Move(dashDirection * dashSpeed * Time.deltaTime);
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
        Vector3 playerPosition = transform.position + Vector3.up * 0.5f;
        Vector3 rightDirection = transform.right * wallRaycastDistance + playerPosition;
        Vector3 rightDiagonalDirection = (transform.right + transform.forward) * wallRaycastDistance + playerPosition;
        Vector3 leftDirection = -transform.right * wallRaycastDistance + playerPosition;
        Vector3 leftDiagonalDirection = (-transform.right + transform.forward) * wallRaycastDistance + playerPosition;

        Debug.DrawLine(playerPosition, rightDirection, Color.yellow);
        Debug.DrawLine(playerPosition, leftDirection , Color.blue);
        Debug.DrawLine(playerPosition, rightDiagonalDirection, Color.yellow);
        Debug.DrawLine(playerPosition, leftDiagonalDirection, Color.blue);


        onLeftWall = false;
        onRightWall = false;
        onFrontWall = false;

        RaycastHit hit;

        if ((Physics.Raycast(playerPosition, transform.right, out hit, wallRaycastDistance,wallLayer)) ||
            (Physics.Raycast(playerPosition, transform.right + transform.forward, out hit, wallRaycastDistance, wallLayer)))
        {
            //Debug.Log("Wall on right");
            onRightWall = true;
            //wallNormal = -transform.right;
            wallNormal = hit.normal;
            //Debug.Log("Normal: " + wallNormal);
        }


        else if ((Physics.Raycast(playerPosition, -transform.right, out hit, wallRaycastDistance, wallLayer)) ||
            (Physics.Raycast(playerPosition, -transform.right + transform.forward, out hit, wallRaycastDistance, wallLayer)))
        {
            //Debug.Log("Wall on left");
            onLeftWall = true;
            //wallNormal = transform.right;
            wallNormal = hit.normal;
            //Debug.Log("Normal: " + wallNormal);
        }

        else if (Physics.Raycast(playerPosition, transform.forward, out hit, wallRaycastDistance, wallLayer))
        {
            //Debug.Log("Wall in front");
            onFrontWall = true;

            //wallNormal = -transform.forward;
            wallNormal = hit.normal;
        }


        if ((onLeftWall || onRightWall || onFrontWall) && !isGrounded) onWall = true;
        else onWall = false;

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
            if (onFrontWall)
            {
                Debug.Log("Climb up the walls");
                verticalVelocity = climbSpeed * speedMultiplier;
            }

            //Wall run
            if ((onLeftWall || onRightWall) && isSprinting)
            {
                Debug.Log("Wallrunning");
                playerController.Move(transform.forward * wallrunBaseSpeed * speedMultiplier * Time.deltaTime);

            }
            */
    }















}

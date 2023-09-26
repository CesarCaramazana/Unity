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
    public bool canMove = true;

    [Header("Horizontal movement")]
    [SerializeField] private float walkSpeed = 8f;
    [HideInInspector] public float speed;// { get; private set; }
    [SerializeField] private float sprintMultiplier = 2f;
    [HideInInspector] public float maxSpeed { get; private set; }
    [SerializeField] private float airMultiplier = 0.8f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] public Vector2 inputDirection { get; private set; }
    [SerializeField] private float acceleration = 2f;
    [SerializeField] public float deceleration { get; private set; } = 2f;
    [SerializeField] private bool accelerationModel = false;


    private float speedMultiplier = 1f;
    private float inputMultiplier = 1f;
    private bool sprint;

    private Vector3 moveDirection;
    [HideInInspector] public Vector3 moveVelocity;
    [HideInInspector] public float verticalVelocity;

    [Header("Vertical movement")]
    [SerializeField] private float initialJumpForce = 8f;
    [SerializeField] private int numberOfJumps = 3;
    [SerializeField] private float jumpCooldown = 0.3f;
    [SerializeField] private bool dampNextJump = true;
    [SerializeField] private float dampJumpFactor = 1.4f;

    [Space(15)]
    [SerializeField] private float gravity = -70f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundRaycastDistance = 0.5f;

    public bool hitGround { get; private set; } = false;
    private bool lastFrameGrounded; 

    private float terminalVelocity = -60f; //Max fall speed


    [HideInInspector] private float jumpForce;
    [HideInInspector] public bool jumped { get; private set; }
    [HideInInspector] public bool canJump { get; private set; }
    [HideInInspector] public int remainingJumps { get; private set; }
    [HideInInspector] public int jumpCount { get; private set; }
    private float timeToJump;

    [Space(10)]
    [Header("Wall jump")]
    [SerializeField] private float wallGravity = -20.0f;
    [SerializeField] private float wallJumpMultiplier = 2f;
    [SerializeField] private LayerMask wallLayer;
    private float wallRaycastDistance = 0.6f;

    private Vector3 wallNormal;

    [Header("Wall run")]
    [SerializeField] private float wallrunBaseSpeed = 10f;
    [SerializeField] private float wallrunFOV = 70f;
    [SerializeField] private float fovChangeSpeed = 5f;
    [SerializeField] private float cameraDutchAngle = 40f;
    [SerializeField] private bool wallrunOnSprint = true;
    [SerializeField] private bool accelerationModelWallrun = true;


    [Header("Dash")]
    [SerializeField] private float dashSpeed = 35f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCooldown = 0.6f;
    private float timeToDash;
    private IEnumerator dashCoroutine;


    [Header("Slopes")]
    [SerializeField] private float slopeRaycastDistance = 0.55f;
    private bool exitingSlope;

    private RaycastHit slopeHit;

    [Header("Grappling hook")]
    [SerializeField] private float grapplingResetJumpTime = 1.5f;
    private float timeGrappling;


    [HideInInspector] public bool isSprinting { get; private set; }
    [HideInInspector] public bool isWalking { get; private set; }
    [HideInInspector] public bool isMoving { get; private set; }
    [HideInInspector] public bool isDashing { get; private set; }
    [HideInInspector] public bool isWallrunning { get; private set; }
    [HideInInspector] public bool isGrappling { get; set; }
    [HideInInspector] public bool isGrounded { get; private set; }
    [HideInInspector] public bool onSlope { get; private set; }
    [HideInInspector] public bool onWall { get; private set; }
    [HideInInspector] public bool onRightWall, onLeftWall, onFrontWall;

    [Header("Lock-on")]
    [SerializeField] public Transform lockTarget;
    [SerializeField] public bool isLockedOn;


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
    [SerializeField] private ParticleSystem hitGroundVFX;
    [SerializeField] private MeshTrailVFX dashVFX;
    [SerializeField] private MeshTrailVFX wallrunVFX;

    [SerializeField] public bool rumble = true;

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
        timeGrappling = 0f;

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
        //playerController.detectCollisions = true;

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
        inputDirection = inputValue.Get<Vector2>().normalized;

        //inputMultiplier = inputValue.Get<Vector2>().magnitude;

        //Debug.Log("Magnitude of input vector: " + inputValue.Get<Vector2>().magnitude);

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
        moveVelocity = new Vector3(0, moveVelocity.y, 0);

        if (!isMoving || !canMove)
        {
            isWalking = isSprinting = isWallrunning = false;
            speed = 0f;

        }
        else
        {
            isSprinting = sprint;
            isWalking = !isSprinting;

            Vector3 horizontalVelocity;
            speedMultiplier = sprint ? sprintMultiplier : 1;

            //Wallrunning
            if (onWall && (!wallrunOnSprint || isSprinting))
            {
                horizontalVelocity = WallrunVelocity();
            }
            //Ground/Air movement
            else
            {
                horizontalVelocity = GroundAirVelocity();
            }

            Vector3 horizontalMoveAmount = horizontalVelocity * Time.deltaTime;
            moveAmount += horizontalMoveAmount;

            //Footstep SFX
            if (footstepsSFX && (isGrounded || isWallrunning) && !isDashing && Time.time > nextFootstepTime)
            {
                //Debug.Log("Next footstep sfx " + (timeBetweenFootsteps / speedMultiplier));
                footstepsSFX.Play();
                nextFootstepTime = Time.time + (timeBetweenFootsteps / speedMultiplier);
            }

            if (!isLockedOn && !isWallrunning) FaceCameraForward();

        }

        //Apply vertical velocity computed in Jump() and Gravity()
        //moveVelocity.y = verticalVelocity;
        Vector3 verticalMoveAmount = new Vector3(0, verticalVelocity, 0) * Time.deltaTime;
        moveAmount += verticalMoveAmount;


        //Finally: move the character controller
        playerController.Move(moveAmount);

        if (isLockedOn && lockTarget != null) LookAtTarget();
        else ResetLookAtTarget();

    }

    private float Accelerate(float currentSpeed, float targetSpeed)
    {
        float newSpeed;

        //Debug.Log("Current speed = " + currentSpeed);
        //Debug.Log("Target speed = " + targetSpeed);
        if (currentSpeed < targetSpeed - 0.1f || currentSpeed > targetSpeed + 0.1f)
        {
            newSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            newSpeed = Mathf.Round(newSpeed * 1000f) / 1000f;
        }
        else
        {
            newSpeed = targetSpeed;
        }
        //Debug.Log("New speed: " + newSpeed);

        return newSpeed;
    }


    private Vector3 GroundAirVelocity()
    {
        //Calculate the movement velocity when not wallrunning, either grounded or airborne
        Vector3 moveVelocity;
        isWallrunning = false;

        //Calculate speed multiplier based on sprinting and airborne
        float airControl = isGrounded ? 1 : airMultiplier;
        float targetSpeed = walkSpeed * speedMultiplier * airControl; // * inputMultiplier;

        if (accelerationModel) speed = Accelerate(speed, targetSpeed);
        else speed = targetSpeed;

        //speed = walkSpeed * speedMultiplier * airControl; // * inputMultiplier;

        //Velocity
        moveVelocity = moveDirection * speed;
        //Debug.Log("Move velocity: " + moveVelocity);

        //Reset wallrun camera effects
        WallrunEffectsDisable();

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

        float targetSpeed = wallrunBaseSpeed * speedMultiplier; // * inputMultiplier;

        if (accelerationModelWallrun) speed = Accelerate(speed, targetSpeed);
        else speed = targetSpeed;

        //Calculate velocity
        moveVelocity = wallrunDirection * speed;

        //Visuals
        WallrunEffectsEnable(polarity);

        return moveVelocity;
    }


    private void WallrunEffectsEnable(int polarity)
    {
        if (wallrunVFX != null) wallrunVFX.Play();
        if (freelookCamera != null)
        {
            //FOV
            float currentFOV = freelookCamera.m_Lens.FieldOfView;
            freelookCamera.m_Lens.FieldOfView = Mathf.Lerp(currentFOV, wallrunFOV, fovChangeSpeed * Time.deltaTime);

            //Center to target
            freelookCamera.m_RecenterToTargetHeading.m_enabled = true;
            freelookCamera.m_YAxisRecentering.m_enabled = true;
            freelookCamera.m_YAxisRecentering.m_RecenteringTime = 0.2f;
            freelookCamera.m_RecenterToTargetHeading.m_RecenteringTime = 0.2f;

            //Dutch angle
            float currentAngle = freelookCamera.m_Lens.Dutch;
            freelookCamera.m_Lens.Dutch = Mathf.Lerp(currentAngle, -polarity * cameraDutchAngle, fovChangeSpeed / 2 * Time.deltaTime);
        }
    }


    private void WallrunEffectsDisable()
    {
        if (wallrunVFX) wallrunVFX.Stop();
        if (freelookCamera != null)
        {
            //FOV
            float currentFOV = freelookCamera.m_Lens.FieldOfView;
            if (currentFOV != normalFOV) freelookCamera.m_Lens.FieldOfView = Mathf.Lerp(currentFOV, normalFOV, fovChangeSpeed * Time.deltaTime);

            //Recenter disabled
            freelookCamera.m_RecenterToTargetHeading.m_enabled = false;
            freelookCamera.m_YAxisRecentering.m_enabled = false;

            //Dutch angle
            float currentAngle = freelookCamera.m_Lens.Dutch;
            freelookCamera.m_Lens.Dutch = Mathf.Lerp(currentAngle, 0f, fovChangeSpeed * Time.deltaTime);
        }


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
                timeGrappling = 0f;
            }
            else if (isGrappling)
            {
                //No gravity while grappling?
                verticalVelocity = 0f;
                timeGrappling += Time.deltaTime;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
                timeGrappling = 0f;
            }
        }
    }

    private void OnJump()
    {
        jumped = true;
    }

    private void Jump()
    {
        canJump = (remainingJumps > 0 && Time.time > timeToJump && canMove);

        if (jumped && canJump)
        {
            exitingSlope = true;

            if (jumpSFX != null) jumpSFX.Play();
            if (jumpCount == numberOfJumps)
            {
                if (jumpVFX != null) jumpVFX.Play();
                if (hitGroundVFX != null) hitGroundVFX.Play();
                if (tripleJumpSFX != null) tripleJumpSFX.Play();
                if (rumble) GamepadRumble.Instance.Play();
            }


            if (onWall)
            {
                //Debug.Log("WALL JUMP!");
                if (jumpVFX != null) jumpVFX.Play();
                //StartCoroutine(WallJump(0.2f));
                ApplyForce(wallNormal, jumpForce * wallJumpMultiplier, deceleration, breakOnJump: false, breakOnWall: false);
            }


            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
            if (dampNextJump) jumpForce = jumpForce * dampJumpFactor;

            remainingJumps--;

            timeToJump = Time.time + jumpCooldown;
        }
    }


    public void ApplyForce(Vector3 direction, float initialSpeed, float deceleration, bool breakOnJump = true, bool breakOnGround = true, bool breakOnWall = true)
    {
        StartCoroutine(ApplyMomentum(direction, initialSpeed, deceleration, breakOnJump, breakOnGround, breakOnWall));
    }

    private IEnumerator ApplyMomentum(Vector3 direction, float initialSpeed, float deceleration, bool breakOnJump, bool breakOnGround, bool breakOnWall)
    {
        float currentSpeed = initialSpeed;
        while (currentSpeed > 0.1)
        {
            //if (isGrounded || jumped || onWall) break;
            if ((breakOnGround && hitGround) || (breakOnJump && (jumped && canJump)) || (breakOnWall && onWall)) break;

            Vector3 moveAmount = direction * currentSpeed * Time.deltaTime;
            playerController.Move(moveAmount);

            currentSpeed = Mathf.Lerp(currentSpeed, 0f, deceleration * Time.deltaTime);
            yield return null;


        }
    }

    private IEnumerator WallJump(float walljumpDuration)
    {
        float currentSpeed = wallJumpMultiplier * jumpForce;

        while (currentSpeed > 0.1)
        {
            if (isGrounded || isMoving) break;

            playerController.Move(wallNormal * currentSpeed * Time.deltaTime);
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, deceleration * Time.deltaTime);
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
        if (Time.time > timeToDash && canMove)
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

        timeGrappling = 0f;
    }

    private void UpdateJumpCount()
    {
        if (jumped) jumped = false;
        jumpCount = numberOfJumps - remainingJumps + 1;

    }

    //CHECKS: OnWall, Grounded, OnSlope

    private void CheckIsGrounded()
    {
        lastFrameGrounded = isGrounded;

        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundRaycastDistance, groundLayer);

        hitGround = (!lastFrameGrounded && isGrounded);
        if (hitGround && hitGroundVFX)
        {
            hitGroundVFX.Play();

        }


        //isGrounded = playerController.isGrounded;

        if ((isGrounded || onWall) && Time.time > timeToJump) ResetJumps();
        if (timeGrappling > grapplingResetJumpTime) ResetJumps();
    }



    private void CheckOnWall()
    {
        Vector3 playerPosition = transform.position + Vector3.up * 0.5f;
        Vector3 rightDirection = transform.right * wallRaycastDistance + playerPosition;
        Vector3 rightDiagonalDirection = (transform.right + transform.forward) * wallRaycastDistance + playerPosition;
        Vector3 leftDirection = -transform.right * wallRaycastDistance + playerPosition;
        Vector3 leftDiagonalDirection = (-transform.right + transform.forward) * wallRaycastDistance + playerPosition;

        Debug.DrawLine(playerPosition, rightDirection, Color.yellow);
        Debug.DrawLine(playerPosition, leftDirection, Color.blue);
        Debug.DrawLine(playerPosition, rightDiagonalDirection, Color.yellow);
        Debug.DrawLine(playerPosition, leftDiagonalDirection, Color.blue);

        onLeftWall = false;
        onRightWall = false;
        onFrontWall = false;

        RaycastHit hit;

        if ((Physics.Raycast(playerPosition, transform.right, out hit, wallRaycastDistance, wallLayer)) ||
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
        if (Physics.Raycast(transform.position + 0.1f * Vector3.up, Vector3.down, out slopeHit, slopeRaycastDistance, groundLayer))
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

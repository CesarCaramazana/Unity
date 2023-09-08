using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GunController))]
[RequireComponent(typeof(MeleeController))]

public class PlayerController_RB : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float rotationSpeed = 6f;
    private float speedMultiplier = 1f;
    [SerializeField] public bool isSprinting;
    [SerializeField] public bool isWalking;
    private Vector3 moveDirection;
    private Vector3 moveVelocity;


    [Header("Dash")]    
    [SerializeField] private float dashTime = 1f;
    [SerializeField] private float dashSpeed = 5f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] public bool isDashing;
    [SerializeField] private AudioEventSO dashSFX;
    private float timeToDash;
    private IEnumerator dashCoroutine;

    [Header("Jump")]
    [SerializeField] private float maxJumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier = 0.8f;
    [SerializeField] public bool isGrounded;

    [Header("Double jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private int numberOfJumps;
    [SerializeField] public int remainingJumps;        
    [SerializeField] private bool dampNextJump;
    [SerializeField] private float dampJumpFactor;
    private float timeToJump;

    [Header("Wall jump")]
    [SerializeField] public bool onWall;
    [SerializeField] private Vector3 wallJumpDirection;
    [SerializeField] private float wallJumpMultiplier = 2f;

    [Header("Slopes")]    
    [SerializeField] public bool onSlope;
    [SerializeField] private float slopeRaycastDistance;
    [SerializeField] private bool exitingSlope;
    private RaycastHit slopeHit;


    [Header("Shoot")]
    [SerializeField] private GunController gunController;
    //[SerializeField] private GunControllerSO gunController;


    [Header("Melee")]
    [SerializeField] private MeleeController meleeController;
    

    [Space(10)]
    [SerializeField] public Rigidbody rb;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private InputManager input;

    [SerializeField] private bool useGamepadRumble=false;


    [Space(10)]
    private bool playerJumped;
    private bool playerDashed;
    private bool playerShot;

    //Singleton
    public static PlayerController_RB Instance;



    // Start is called before the first frame update
    void Start()
    {

        Instance = this;

        //gunController = GetComponent<GunControllerSO>();
        gunController = GetComponent<GunController>();
        meleeController = GetComponent<MeleeController>();

        remainingJumps = numberOfJumps;
        timeToJump = Time.time;
        timeToDash = Time.time;

        
    }

    // Update is called once per frame
    void Update()
    {
        //Check if on slope and grounded
        OnSlope();
        CheckIsGrounded();

        //Jump
        Jump();

        //Horizontal movement
        GetMovementDirection();        
        Sprinting();        
        Dash();
        SpeedControl();

        //Attacks
        MeleeAttack();
        Shoot();        

        
    }


    private void FixedUpdate()
    {       
        Move();
    }


    private void OnCollisionEnter(Collision collision)
    {

    }

    private void OnCollisionStay(Collision collision)
    {
        //Debug.Log("Player collision stay");

        CheckOnWallCollision(collision);

    }

    private void CheckOnWallCollision(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            //Debug.Log("Hitting a WALL");
            Vector3 collisionPoint = collision.contacts[0].point;
            Vector3 normalDirection = collision.contacts[0].normal;

            //Debug.DrawLine(collisionPoint, normalDirection + collisionPoint, Color.blue);

            //print("Collision point: " + collisionPoint);
            //print("Normal vector: " + normalDirection);

            onWall = true;
            wallJumpDirection = normalDirection;

        }
    }

    private void OnCollisionExit(Collision collision)
    {
        onWall = false;
    }

    private void GetInput()
    {
        playerJumped = input.GetJumpPerformed();
        playerDashed = input.GetDashPerformed();
        playerShot = input.GetShootPerformed();
    }


    private void Sprinting()
    {
        isSprinting = input.GetSprintPerformed();

        speedMultiplier = isSprinting ? sprintMultiplier : 1;
        isWalking = isSprinting ? false : true;  

    }


    private IEnumerator GamepadRumble(float lowFreq, float highFreq, float rumbleDuration = 0.2f)
    {
        Gamepad.current.SetMotorSpeeds(lowFreq, highFreq);
        yield return new WaitForSeconds(rumbleDuration);
        Gamepad.current.SetMotorSpeeds(0, 0);
    }

    private void Jump()
    {
        playerJumped = input.GetJumpPerformed();
       

        if (playerJumped && remainingJumps > 0 && Time.time > timeToJump)
        {           
            exitingSlope = true;

            Vector3 jumpDirection;
            if (onWall && !isGrounded)
            {
                jumpDirection = (wallJumpDirection + Vector3.up).normalized;
                rb.AddForce(jumpDirection * jumpForce * wallJumpMultiplier, ForceMode.Impulse);
                //transform.forward = wallJumpDirection;
            }
            else
            {
                jumpDirection = Vector3.up;
                rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
            }

            //Normal up jump
            //rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            //Walljump consideration
            //rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
            if (dampNextJump) jumpForce = jumpForce * dampJumpFactor;
            remainingJumps--;
            
            timeToJump = Time.time + jumpCooldown;
        }


        if ((isGrounded || onWall) && Time.time > timeToJump) ResetJumps();

    }


    private void GetMovementDirection()
    {
        //Get input direction in 2D
        Vector2 inputDirection = input.GetMovementVectorNormalized();

        //Cast input direction into a 3D vector
        moveDirection = new Vector3(inputDirection.x, 0.0f, inputDirection.y);

        //Rotate the vector with respect to the camera
        moveDirection = Quaternion.Euler(0f, mainCamera.eulerAngles.y, 0f) * moveDirection;
        moveDirection = moveDirection.normalized;

        //Project onto slopes
        if (onSlope && !exitingSlope) moveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);

    }


    private void Move()
    {

        //Velocity
        Vector3 moveAmount = moveDirection * moveSpeed * speedMultiplier;

        //print(playerVelocity);

        if (!isGrounded)
        {
            rb.AddForce(moveAmount * airMultiplier, ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(moveAmount, ForceMode.Impulse);
        }

        RotatePlayer();


    }


    private void RotatePlayer()
    {
        if (moveDirection != Vector3.zero)
        {
            Quaternion rotation = Quaternion.Euler(0f, mainCamera.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        }

    }


    private void SpeedControl()
    {        
        
        if (onSlope && !exitingSlope)
        {
            float velocityMagnitude = rb.velocity.magnitude;
            if (velocityMagnitude > moveSpeed)
            {
                //Debug.Log("Speed control: Limiting velocity in XYZ");
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }

        }       

        
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);

        if (flatVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
        
    }


    private void Dash()
    {
        bool dashTriggered = input.GetDashPerformed(); // A un evento

        //Debug.Log(dashTriggered);

        // Dash coroutine
        if (dashTriggered && Time.time > timeToDash)
        {

            if (dashCoroutine != null)
            {
                StopCoroutine(dashCoroutine);
            }

            dashCoroutine = DashCoroutine(dashTime, dashSpeed);
            StartCoroutine(dashCoroutine);

            timeToDash = Time.time + dashCooldown;
        }


    }

    IEnumerator DashCoroutine(float dashTime, float dashSpeed)
    {
        float startTime = Time.time;

        //Direction from input
        Vector2 inputDirection = input.GetMovementVectorNormalized();
        Vector3 dashDirection;

        //AudioManager.instance.Play("Dash");
        dashSFX.Play();

        //If no input was provided -> Dash forward
        if (inputDirection == Vector2.zero)
        {
            dashDirection = rb.transform.forward;
        }
        else
        {
            //dashDirection = Quaternion.Euler(0f, mainCamera.eulerAngles.y, 0f) * new Vector3(inputDirection.x, 0.0f, inputDirection.y);
            dashDirection = moveDirection;
        }

        isDashing = true;
        while (Time.time < startTime + dashTime)
        {
            rb.AddForce(dashDirection * dashSpeed, ForceMode.Impulse);
            yield return null;
        }
        isDashing = false;

    }


    private void ResetJumps()
    {
        remainingJumps = numberOfJumps;
        jumpForce = maxJumpForce;
        exitingSlope = false;
    }


    private void CheckIsGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.2f);


    }


    // AIMING AND SHOOTING
    private void Shoot()
    {
        bool playerShoot = input.GetShootPerformed();
        bool playerShootReleased = input.GetShootReleased();
        bool switchGun = input.GetSwitchGunPerformed();

        if (playerShoot)
        {
            //Debug.Log("Player shoot!");
            //gunController.Shoot();
            gunController.OnTriggerHold();

            if (useGamepadRumble) StartCoroutine(GamepadRumble(0.25f, 0.5f));

            //Vector3 recoilVelocity = -rb.transform.forward * knockbackForce;
            //rb.AddForce(recoilVelocity, ForceMode.Impulse);
        }

        else if (playerShootReleased)
        {
            gunController.OnTriggerRelease();
        }


        if (switchGun)
        {
            //Debug.Log("SWITCH WEAPON");
            gunController.SwitchGun();
        }

    }

    // MELEE

    private void MeleeAttack()
    {
        bool lightAttack = input.GetLightAttackPerformed();
        bool heavyAttack = input.GetHeavyAttackPerformed();

        if (lightAttack)
        {
            meleeController.MeleeAttack(MeleeAttackType.Light);
        }
        else if (heavyAttack)
        {
            meleeController.MeleeAttack(MeleeAttackType.Heavy);
        }
    }



    private void OnSlope()
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

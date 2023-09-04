using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Windows;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GunController))]
[RequireComponent(typeof(MeleeController))]

public class PlayerController : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintMultiplier = 2f;
    
    private float speedMultiplier = 1f;
    [SerializeField] public bool isSprinting;
    [SerializeField] public bool isWalking;
    private Vector3 moveDirection;
    private Vector3 moveVelocity;


    [Header("Player-to-camera Rotation")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Dash")]
    [SerializeField] private float dashTime = 0.5f;
    [SerializeField] private float dashSpeed = 10f;
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
    private bool jumped;

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
    private bool isShooting = false;
    //[SerializeField] private GunControllerSO gunController;


    [Header("Melee")]
    [SerializeField] private MeleeController meleeController;


    [Space(10)]
    [SerializeField] public Rigidbody rb;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private PlayerInput playerInput;

    //Singleton
    public static PlayerController Instance;


    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        //gunController = GetComponent<GunControllerSO>();
        gunController = GetComponent<GunController>();
        meleeController = GetComponent<MeleeController>();
        playerInput = GetComponent<PlayerInput>();

        remainingJumps = numberOfJumps;
        timeToJump = Time.time;
        timeToDash = Time.time;


    }

    // Update is called once per frame
    void Update()
    {
        CheckOnSlope();
        CheckIsGrounded();

        SpeedControl();        

        Shoot();
    }


    private void FixedUpdate()
    {
        Jump();
        Move();
    }

    
    private void OnJump(InputValue inputValue)
    {
        jumped = inputValue.isPressed;
    }

    private void Jump()
    {

        if (jumped && remainingJumps > 0 && Time.time > timeToJump)
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

            if (dampNextJump) jumpForce = jumpForce * dampJumpFactor;
            remainingJumps--;

            timeToJump = Time.time + jumpCooldown;
        }
        jumped = false;
    }



    //Get input direction
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

        //Debug.Log("Input direction = " + inputDirection);
    }

    //Movement logic
    private void Move()
    {
        //Check if player is walking or sprinting
        speedMultiplier = isSprinting ? sprintMultiplier : 1;
        isWalking = (!isSprinting && moveDirection != Vector3.zero);

        //Velocity
        Vector3 moveAmount = moveDirection * moveSpeed * speedMultiplier;

        if (!isGrounded)
        {
            rb.AddForce(moveAmount * airMultiplier, ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(moveAmount, ForceMode.Impulse);
        }


        if (moveDirection != Vector3.zero) RotatePlayer();

    }

    //Limit speed
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

    private void RotatePlayer()
    {
        //Rotates the player to the camera forward when we try to move forward

        float targetAngle;

        //targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
        //targetAngle = mainCamera.eulerAngles.y;
        targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

        Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        
        
    }


    private void OnLockOnTarget()
    {
        Debug.Log("Lock on target R3");
    }


    private void OnSprint(InputValue inputValue)
    {
        //Debug.Log("Sprint");
        isSprinting = inputValue.isPressed;
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

            dashCoroutine = DashCoroutine(dashTime, dashSpeed);
            StartCoroutine(dashCoroutine);

            timeToDash = Time.time + dashCooldown;
        }
    }


    IEnumerator DashCoroutine(float dashTime, float dashSpeed)
    {
        float startTime = Time.time;

        //Direction from input
        Vector3 dashDirection;

        if (dashSFX != null) dashSFX.Play();

        //If no input was provided -> Dash forward
        if (moveDirection == Vector3.zero)
        {
            dashDirection = rb.transform.forward;
        }
        else
        {
            dashDirection = moveDirection;
        }

        isDashing = true;
        while (Time.time < startTime + dashTime /*&& Time.timeScale != 0*/) //the timeScale != 0 breaks the coroutine, so all momentum is lost
        {
            rb.AddForce(dashDirection * dashSpeed, ForceMode.Impulse);
            yield return new WaitForSeconds(Time.deltaTime);
            //yield return null; //Does not freeze with timeScale
        }
        isDashing = false;

    }


    private void OnShoot(InputValue inputValue)
    {
        //Debug.Log("Shoot");
        isShooting = inputValue.isPressed;
        //Debug.Log(inputValue.isPressed);
    }


    private void Shoot()
    {
        if (isShooting)
        {
            gunController.OnTriggerHold();
        }
        else
        {
            gunController.OnTriggerRelease();
        }
    }




    private void OnLightMeleeAttack()
    {
        //Debug.Log("Light attack");
        meleeController.MeleeAttack(MeleeAttackType.Light);
    }

    private void OnHeavyMeleeAttack()
    {
        //Debug.Log("Heavy attack");
        meleeController.MeleeAttack(MeleeAttackType.Heavy);
    }

    private void OnSwitchGun()
    {
        //Debug.Log("Switch gun");
        gunController.SwitchGun();
    }

    private void OnSwitchSword()
    {
        //Debug.Log("Switch sword");
        meleeController.SwitchSword();
    }


    //-----------------------------

    private void ResetJumps()
    {
        remainingJumps = numberOfJumps;
        jumpForce = maxJumpForce;
        exitingSlope = false;
    }


    private void CheckIsGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.2f);
        if ((isGrounded || onWall) && Time.time > timeToJump) ResetJumps();
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



    private void OnCollisionStay(Collision collision)
    {
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
   

}

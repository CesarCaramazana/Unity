using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GunController))]

public class PlayerController_RB : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] public bool isSprinting;


    [Header("Dash")]    
    [SerializeField] private float dashTime = 1f;
    [SerializeField] private float dashSpeed = 5f;
    [SerializeField] private float dashCooldown = 1f;
    private float timeToDash;
    private IEnumerator dashCoroutine;

    [Header("Jump")]
    [SerializeField] private float jumpForce;    
    [SerializeField] private int numberOfJumps;
    [SerializeField] private int remainingJumps;    
    [SerializeField] private float jumpCooldown;
    [SerializeField] public bool isGrounded;
    private float timeToJump;

    [Header("Slopes")]
    [SerializeField] private float maxSlopeAngle;
    [SerializeField] private RaycastHit slopeHit;
    [SerializeField] public bool onSlope;


    [Header("Shoot")]
    [SerializeField] private GunController gunController;
    [SerializeField] private float recoilForce; //This cannot go here but in the gun or controller script


    [Header("Melee")]
    [SerializeField] private MeleeController meleeController;
    

    [Space(10)]
    [SerializeField] public Rigidbody rb;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private InputManager input;


    [Space(10)]
    private bool playerJumped;
    private bool playerDashed;
    private bool playerShot;



    // Start is called before the first frame update
    void Start()
    {
        remainingJumps = numberOfJumps;
        timeToJump = Time.time;
        timeToDash = Time.time;
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckIsGrounded();
        Sprinting();
        Jump();
        Dash();

        MeleeAttack();
        Shoot();

        OnSlope();
        SpeedControl();




    }


    private void FixedUpdate()
    {       
        Move();
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

        if (isSprinting)
        {
            speedMultiplier = 2.3f;
        }
        else
        {
            speedMultiplier = 1f;
        }
    }

    private void Jump()
    {
        playerJumped = input.GetJumpPerformed();

        // reset y velocity
        //rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (isGrounded) ResetJumps();

        if (playerJumped && remainingJumps > 0 && Time.time > timeToJump)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            remainingJumps--;
            timeToJump = Time.time + jumpCooldown;
        }

    }


    private void Move()
    {
        //Get input direction in 2D
        Vector2 inputDirection = input.GetMovementVectorNormalized();

        //Cast input direction into a 3D vector
        Vector3 moveDirection = new Vector3(inputDirection.x, 0.0f, inputDirection.y);
        //Debug.Log("Movement direction: " + moveDirection);

        //Rotate the vector with respect to the camera
        moveDirection = Quaternion.Euler(0f, mainCamera.eulerAngles.y, 0f) * moveDirection;
        moveDirection = moveDirection.normalized;

        //Project onto slopes
        if (onSlope)
        {
            Debug.Log("On slope");
            moveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
        }

        //Velocity
        Vector3 playerVelocity = moveDirection * moveSpeed * speedMultiplier;

        //print(playerVelocity);

        //Move amount: velocity * time
        Vector3 moveAmount = playerVelocity;

        rb.AddForce(moveAmount, ForceMode.VelocityChange);
        //rb.AddForce(moveAmount, ForceMode.Force);

    }


    private void SpeedControl()
    {        
        /*
        if (onSlope)
        {
            float velocityMagnitude = rb.velocity.magnitude;
            if (velocityMagnitude > moveSpeed)
            {
                Debug.Log("Speed control: Limiting velocity in XYZ");
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }

        }*/
        

        
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

        //If no input was provided -> Dash forward
        if (inputDirection == Vector2.zero)
        {
            dashDirection = rb.transform.forward;
        }
        else
        {
            dashDirection = Quaternion.Euler(0f, mainCamera.eulerAngles.y, 0f) * new Vector3(inputDirection.x, 0.0f, inputDirection.y);
        }

        while (Time.time < startTime + dashTime)
        {
            rb.AddForce(dashDirection * dashSpeed, ForceMode.Force);
            yield return null;
        }


    }


    private void ResetJumps()
    {
        remainingJumps = numberOfJumps;
    }


    private void CheckIsGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.2f);


    }



    // AIMING AND SHOOTING
    private void Shoot()
    {
        bool playerShoot = input.GetShootPerformed();
        bool switchGun = input.GetSwitchGunPerformed();

        if (playerShoot)
        {
            //Debug.Log("Player shoot!");
            gunController.Shoot();

            //Vector3 recoilVelocity = -rb.transform.forward * recoilForce;
            //rb.AddForce(recoilVelocity, ForceMode.Impulse);
        }


        if (switchGun)
        {
            Debug.Log("SWITCH WEAPON");
            gunController.SwitchGun();
        }

    }

    // MELEE
    private void LightAttack()
    {
        bool lightAttack = input.GetLightAttackPerformed();

        if (lightAttack)
        {
            meleeController.LightAttack();
        }

    }

    private void HeavyAttack()
    {
        bool heavyAttack = input.GetHeavyAttackPerformed();

        if (heavyAttack)
        {
            meleeController.HeavyAttack();
        }

    }

    private void MeleeAttack()
    {
        LightAttack();
        HeavyAttack();
    }






    private void OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 0.1f))
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player movement")]
    public CharacterController characterController;

    [SerializeField] private float moveSpeed = 1f;

    [Header("Dash")]
    [SerializeField] private float dashTime = 1f;
    [SerializeField] private float dashSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private bool isGrounded;

    private Vector3 playerVelocity = Vector3.zero;
    private IEnumerator dashCoroutine;


    [Header("Slopes")]
    [SerializeField] private float maxSlopeAngle;
    [SerializeField] private RaycastHit slopeHit;
    //[SerializeField] private bool onSlope;




    [Header("Shooting")]
    [SerializeField] private GunController gunController;


    [Header("Camera")]
    [SerializeField]
    private Transform mainCamera;

    [Space(20)]
    public InputManager input;


    private void Start()
    {
    }

    private void Awake()
    {
    }


    private void FixedUpdate()
    {
        //Shoot();
    }

    // Update is called once per frame
    void Update()
    {
        //OnSlope();
        Jump();
        Move();
        Shoot();

        

        bool dashTriggered = input.GetDashPerformed(); // A un evento

        //Debug.Log(dashTriggered);


        
        // Dash coroutine
        if (dashTriggered)
        {
            if (dashCoroutine != null)
            {
                StopCoroutine(dashCoroutine);
            }

            dashCoroutine = Dash(dashTime, dashSpeed);
            StartCoroutine(dashCoroutine);
        }
        
    }


    private void Jump()
    {
        bool playerJumped = input.GetJumpPerformed();
        isGrounded = characterController.isGrounded;

        if (playerVelocity.y < 0f && isGrounded) playerVelocity.y = -2f;


        //Apply vertical velocity if the jump button has been pressed
        if (playerJumped)
        {
            Debug.Log("JUMP!");
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2f * gravity);

        }


        //Apply gravity every frame
        playerVelocity.y += gravity * Time.deltaTime;

        //print(playerVelocity);

        //characterController.Move(playerVelocity * Time.deltaTime);



    }


    private void Move()
    {
        //Get input direction in 2D
        Vector2 inputDirection = input.GetMovementVectorNormalized();
        //Debug.Log("Input direction: " + inputDirection);

        //Cast input direction into a 3D vector
        Vector3 moveDirection = new Vector3(inputDirection.x, 0.0f, inputDirection.y);
        //Debug.Log("Movement direction: " + moveDirection);

        //Rotate the vector with respect to the camera
        moveDirection = Quaternion.Euler(0f, mainCamera.eulerAngles.y, 0f) * moveDirection;
        moveDirection = moveDirection.normalized;

        //Project onto slopes
        /*if (OnSlope())
        {
            Debug.Log("On slope");
            moveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
        }*/


        //Calculate velocity
        playerVelocity.x = moveDirection.x * moveSpeed;
        playerVelocity.z = moveDirection.z * moveSpeed;
        //Vector3 velocity = moveDirection * moveSpeed;
        //Debug.Log("Velocity: " + velocity);

        

        //Calculate moveAmount
        //Vector3 moveAmount = Vector3.Scale(velocity * Time.deltaTime, new Vector3(1,0,1));
        Vector3 moveAmount = playerVelocity * Time.deltaTime;

        //Debug direction
        //Debug.DrawLine(transform.position, transform.position + playerVelocity, Color.red);
        Debug.Log("Velocity = " + playerVelocity);
        

        //Move!
        characterController.Move(moveAmount);

        //characterController.Move((moveDirection * moveSpeed * Time.deltaTime) + new Vector3(0, playerVelocity.y, 0) * Time.deltaTime);

    }





    private void Shoot()
    {
        bool playerShoot = input.GetShootPerformed();

        if (playerShoot)
        {
            //Debug.Log("Player shoot!");
            gunController.Shoot();
        }
        
    }




    IEnumerator Dash(float dashTime, float dashSpeed)
    {
        //Debug.Log("Dash coroutine");

        float startTime = Time.time;
        //Direction from forward
        //Vector3 dashDirection = characterController.transform.forward;

        //Direction from input
        Vector2 inputDirection = input.GetMovementVectorNormalized();
        Vector3 dashDirection = Quaternion.Euler(0f, mainCamera.eulerAngles.y, 0f) * new Vector3(inputDirection.x, 0.0f, inputDirection.y);

        while (Time.time < startTime + dashTime)
        {
            characterController.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }


        
    }








}

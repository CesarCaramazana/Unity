using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    

    [SerializeField]
    private float playerSpeed = 10.0f;
    [SerializeField]
    private float jumpHeight = 5.0f;
    [SerializeField]
    private float gravityValue = -9.81f;


    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    private InputManager inputManager;


    private void Start()
    {
        controller = GetComponent<CharacterController>();
        inputManager = InputManager.Instance; //Create the input manager object
    }

    void Update()
    {
        // 1. Check if the player is grounded
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; //if the player is grounded, set Y velocity to zero (-2f so that the CheckSphere can reach the ground).
        }


        // 2. Movement
        //Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")); //Old input system
        // 2.1) Call the input manager GetPlayerMovement()
        Vector2 movement = inputManager.GetPlayerMovement();
        Vector3 move = new Vector3(movement.x, 0.0f, movement.y).normalized; // 2.2) Cast the Vector2 to Vector3
        move = transform.right * movement.x + transform.forward * movement.y;

        // 2.3) Move the player in the direction
        controller.Move(move.normalized * Time.deltaTime * playerSpeed);

        // 2.4) Make the player face the direction 
        /*if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }*/

        // 3. Jump
        //if (Input.GetButtonDown("Jump") && groundedPlayer) //Old input system
        //if (inputManager.PlayerJumpedThisFrame() && groundedPlayer)
        if (inputManager.PlayerJumpedThisFrame())
            {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}

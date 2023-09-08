using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    //Initialize the player controls
    private PlayerControls playerControls;

    //Singleton: an unique instance of Input Manager
    private static InputManager _instance;
    public static InputManager Instance
    {
        get { return _instance; }
    }

    //On awake, instantiate the controls
    private void Awake()
    {
        // Controls
        playerControls = new PlayerControls();

        // Input Manager: we can only have one, so check if there's already an instance
        if (_instance != null && _instance != this) 
        {
            Destroy( _instance.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    //Enable/Disable 
    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    //Now we need helper functions so that in other scripts we can obtain the values from the playerControls

    // playerControls.[Action Maps].[Actions].[Read the output]<>();

    // 1. Movement
    public Vector2 GetPlayerMovement()
    {
        return playerControls.FirstPersonGameplay.Movement.ReadValue<Vector2>(); // These must be the names we used in the Input Actions scheme.

    }

    // 2. Look
    public Vector2 GetMouseDelta()
    {
        return playerControls.FirstPersonGameplay.Look.ReadValue<Vector2>();
    }

    // 3. Jump
    public bool PlayerJumpedThisFrame()
    {
        return playerControls.FirstPersonGameplay.Jump.triggered; //triggered: true if the button was pressed on the frame

    }

}

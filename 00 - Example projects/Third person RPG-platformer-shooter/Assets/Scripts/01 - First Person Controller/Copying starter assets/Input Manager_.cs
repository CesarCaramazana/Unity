using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager_ : MonoBehaviour
{

    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;
    public bool dodge;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    //These functions should be called "On[Action]", with the [Action]s defined in the Input Actions scheme
    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }

    public void OnLook(InputValue value)
    {
        if (cursorInputForLook)
        {
            LookInput(value.Get<Vector2>());
        }
    }

    public void OnJump(InputValue value)
    {
        JumpInput(value.isPressed);
        Debug.Log("On jump");
    }

    public void OnSprint(InputValue value)
    {
        SprintInput(value.isPressed);
    }

    public void OnDodge(InputValue value)
    {
        DodgeInput(value.isPressed);
    }

    public void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }

    public void LookInput(Vector2 newLookDirection)
    {
        look = newLookDirection;
    }

    public void JumpInput(bool newJumpState)
    {
        jump = newJumpState;
    }

    public void SprintInput(bool newSprintState)
    {
        sprint = newSprintState;
    }

    public void DodgeInput(bool newDodgeState)
    {
        dodge = newDodgeState;
    }

    //One function per action to read the values
    /*
    public Vector2 GetPlayerMovement()
    {
        move = playerControls.Groundcontrols.Move.ReadValue<Vector2>();
        return move;
    }

    public Vector2 GetMouseDelta()
    {
        look = playerControls.Groundcontrols.Look.ReadValue<Vector2>();
        return look;
    }

    public bool GetPlayerJump()
    {
        jump = playerControls.Groundcontrols.Jump.triggered;
        Debug.Log(jump);
        return jump;
    }

    public bool GetPlayerSprint()
    {
        sprint = playerControls.Groundcontrols.Sprint.triggered;
        return sprint;
    }

    public bool GetPlayerDodge()
    {
        dodge = playerControls.Groundcontrols.Dodge.triggered;
        return dodge;
    }
    */



}

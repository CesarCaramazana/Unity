using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    private PlayerControls playerControls;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }


    // Move direction
    public Vector2 GetMovementVectorNormalized()
    {
        return playerControls.Player.Move.ReadValue<Vector2>().normalized;
    }

    // Aim direction
    public Vector2 GetAimingVectorNormalized()
    {
        return playerControls.Player.Look.ReadValue<Vector2>().normalized;
    }

    // Jump triggered
    public bool GetJumpPerformed()
    {
        return playerControls.Player.Jump.triggered; 
    }

    // Dash triggered
    public bool GetDashPerformed()
    {
        return playerControls.Player.Dash.triggered;
    }

    //Sprint
    public bool GetSprintPerformed()
    {
        return playerControls.Player.Sprint.IsPressed();
    }

    //Enemy lock
    public bool GetLockToEnemyPerformed()
    {
        return playerControls.Player.LockOn.triggered;
    }

    //Shooting
    public bool GetShootPerformed()
    {
        return playerControls.Player.Shoot.IsPressed();
    }

    public bool GetShootReleased()
    {
        return playerControls.Player.Shoot.WasReleasedThisFrame();
    }

    //Switch gun
    public bool GetSwitchGunPerformed()
    {
        return playerControls.Player.SwitchGun.triggered;
    }

    //Melee
    //Light attack
    public bool GetLightAttackPerformed()
    {
        return playerControls.Player.LightMeleeAttack.triggered;
    }
    //Heavy attack
    public bool GetHeavyAttackPerformed()
    {
        return playerControls.Player.HeavyMeleeAttack.triggered;

    }
    //Switch sword
    public bool GetSwitchSwordPerformed()
    {
        return playerControls.Player.SwitchSword.triggered;
    }


    //Grapple hook
    public bool GetGrapplePerformed()
    {
        return playerControls.Player.Grapple.triggered;
    }



    public bool GetTogglePausePerformed()
    {
        return playerControls.Player.Pause.triggered;
    }




    //-----
    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }


}

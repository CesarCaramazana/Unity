# A showcase of Player movement options for Rigidbody-based controller

NOTE: the PlayerController_InputManager.cs script uses the C# class InputManager approach, while the PlayerController_PlayerInput.cs uses the PlayerInput component, with the Send Messages behavior, which allows to retrieve the inputs on functions that are named "On[ActionName]", such as "OnMove(InputValue value)".


The movement options currently implemented:

- Basic horizontal movement with AddForce(), speed limiting and slope control. Player's forward aligns with camera.
- Sprinting: modifying the speed on button held.
- Dash: with a coroutine.
- Jump/Double jump: arbitrary number of jumps with option for dampening subsequent jumps, wall jump (a bit rudimentary) and ground check.
- Grapple hook: (rudimentary) with Spring Joint. Grapple is thrown with a raycast from the camera.

Abstract support for a Melee and Gun controller.

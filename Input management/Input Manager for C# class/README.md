# An Input Manager implementation for polling input actions

An input manager script that allows retrieving the input from the Player Actions asset (from the New Input System package) if we choose the option "Generate C# class" when creating the asset.

With this approach we can create the functions that deal with the input all in one place, and in the Player Controller we can just reference the InputManager object and call its functions to obtain the return values.
We don't make use of the PlayerInput component. 

Pros:
- We can define functions for different actions (such as Button.isPressed, Button.WasReleasedThisFrame, action.triggered, or ReadValue<Vector2>).


Cons:
- This an input polling approach: we check all the inputs on Update(), every frame, so if we drop the frames where inputs were provided, we miss the input.

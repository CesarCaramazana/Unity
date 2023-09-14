# Gamepad rumble (vibration)

Gamepad rumble provides haptic feedback to the player and can be used to enhance the gamefeel of certain actions. 

Unity provides an easy way to implement this system by simply accessing the current ```Gamepad``` and triggering the function ```SetMotorSpeeds()```, with values for the low frequencies and high frequencies intensity.
This implementation overloads a ```Play()``` function with different input parameters, depending if you just want to trigger the effect with default intensity and duration, or customize them depending on the action.

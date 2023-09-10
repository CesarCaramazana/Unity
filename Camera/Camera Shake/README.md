# Camera shake effect with Cinemachine

Camera shake is a nice visual effect that increases the game-feel of impactful actions, such as shooting or explosions. As the name suggests, it consists on rapidly changing the position of the camera for a brief period of time as a response to particular events, simulating that the camera is a physical element on the game world. 

With Cinemachine we can create this effect by means of the Basic Multichannel Perlin component, which is used to create handholding effects. It is just a matter of setting the values for frequency and gain and reseting them after a certain duration.

In this implementation, since I've been using two types of Cinemachine cameras, FreeLook and Virtual, I defined two coroutines to achieve the effect, as the FreeLook camera has three orbital rigs that have their own Multichannel Perlin. 


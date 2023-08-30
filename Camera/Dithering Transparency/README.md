# A dithering transparency solution to camera-player obstacle occlusion

The implementation of the camera-player obstacle detection can be found in the [LlamAcademy repository](https://github.com/llamacademy/urp-fading-standard-shaders/tree/main/Assets/Scripts), aswell as in the [tutorial](https://www.youtube.com/watch?v=vmLIy62Gsnk&ab_channel=LlamAcademy).

The custom dithering shader was implemented by [Cheddar Game Dev](https://www.youtube.com/watch?v=qk6nrQihcOQ&ab_channel=CheddarGameDev).

## The Camera-Player occlusion problem

In a third-person controller set up, it is a common situation that obstacles get in between the player and the camera, which hinders visibility and overall hurts the gameplay experience. 
One solution to this problem is adding a collider to the camera and configure a behavior in the Cinemachine brain so that it gets adjusted on collisions with objects. An example: Dark Souls' camera (which sometimes is the real enemy).

Other solution, that can be found in games such as Super Mario Odyssey or Devil May Cry, is detecting the objects between player and camera and apply transparency to their materials, without the need of modifying the camera's position.
In this case scenario, two options are available:
- Alpha transparency: reducing the alpha value of the material.
- Dithering transparency: applying a checkerboard pattern on the material so that we "decimate" the surface.

## Dithering Transparency

(To do: image example and explanation)

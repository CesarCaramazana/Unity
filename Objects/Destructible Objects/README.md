# Destructible Objects

The implementation is based on [Brackeys](https://www.youtube.com/watch?v=EgNV0PWVaS8&ab_channel=Brackeys)'s tutorial.

## Problem definition

Destructible objects provide a fun way in which the player can interact with the objects of a scene. The main idea is that the player can damage an object to the point that it shatters into little pieces. 
There are many games that have this mechanic, such as Resident Evil II's wooden boxes, or the many environmental objects in Bayonetta.

## Solution

This solution relies on having two copies of the object: one whole and one shattered. There are other solutions that rely on cutting the meshes at runtime, but are much more costly. Given that usually assets do not come with their shattered twin, Blender provides a very useful tool named *Cell Fracture*. An example of its usage: [Markom3D channel](https://www.youtube.com/watch?v=E2WLmw2Crcs&ab_channel=Markom3D).

Now, triggering the effect consists on destroying the original object and instantiating the shattered copy in the exact position. Each little piece has a rigidbody component, **a convex mesh** and the ShatteredPiece.cs script, which applies an explosive force on enable.

I have made the destructible objects "living entities" (```LivingEntity.cs```), which inherit the IDamageable interface. Thus, these objects can take hits (```TakeHit(float damage)``` that diminish their health value. Upon death, the function ```Shatter()```, from the DestructibleObject class, is called. 

To make the vanishing of the pieces smoother, the materials fade into black.

![Example](https://github.com/CesarCaramazana/Unity/blob/main/Objects/Destructible%20Objects/Images/destructible_example.PNG)

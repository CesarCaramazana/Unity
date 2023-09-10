# Destructible Objects

The implementation is based on [Brackeys](https://www.youtube.com/watch?v=EgNV0PWVaS8&ab_channel=Brackeys)'s tutorial.

## Problem definition

Destructible objects provide a fun way in which the player can interact with the objects of a scene. The main idea is that the player can damage an object to the point that it shatters into little pieces. 
There are many games that have this mechanic, such as Resident Evil II's wooden boxes, or the many environmental objects in Bayonetta.

## Solution

This solution relies on having two copies of the object: one whole and one shattered. There are other solutions that rely on cutting the meshes at runtime, but are much more costly. Given that usually assets do not come with their shattered twin, Blender provides a very useful tool named *Cell Fracture*. An example of its usage: [Markom3D channel](https://www.youtube.com/watch?v=E2WLmw2Crcs&ab_channel=Markom3D).

# Lock-on System with Cinemachine

The implementation of this lock on system builds upon the tutorial by [Game Factory](https://www.youtube.com/watch?v=ozb6aFTwiHU&ab_channel=GameFactory), with a few modifications and improvements that better suit my case scenario.


## Problem definition
A lock-on system is a mechanic that allows the camera to focus on a single target, usually so that it remains at the center of the screen in 3rd person games, taking control of the camera away from the player. Here we are refering to a **camera behavior**, rather than a soft-lock system of player magnetism (such as the one in the Batman Arkham trilogy). 
Some examples of games that have a camera lock-on system similar to what we are trying to accomplish: Dark Souls, God of War 2018 or Nier Automata.

The main problematiques that have to be addressed in a lock-on system are:
- How to detect nearby enemies (potential targets).
- How to select the target among all possibilities.
- How to set up the camera to focus on the target.
- How to provide visual feedback on the target selected (UI reticle).
- How the player movement is modified when locked on.


## Solution

### Target detection and selection
First, we cast a sphere with a certain radius from the player, that returns all colliders with the enemy layer. Then, compute either 1) distance to the player or 2) angle with respect to the camera forward, aswell as a bool to determine if the potential target is on sight, that is, if the linecast between player/camera and target does not hit any obstacle.
Select the target with minimum distance/angle that is on sight. For this target, we calculate the half-height of the capsule collider so that we can apply an offset in the Y-axis (so that the target point is in the middle of the object).


### Camera behavior
We set up a Cinemachine Virtual camera that **follows** the player and **looks at** the target. When the target has been selected, we increase the ```Priority``` of the Lock-on Camera with respect to our Free Look camera (unlocked). 

```
lockCamera.m_LookAt = lockTarget;
lockCamera.Priority = 11;
lockCamera.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.y = lockYOffset;
```


### UI reticle
When locked on, a raycast is thrown from the player to the target. We detect the hit point on the enemy, offset the distance a little bit towards the player to avoid Z-fighting, and place a sprite renderer that always looks at the camera.

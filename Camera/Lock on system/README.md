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
First, we cast a sphere with a certain radius from the player, that returns all colliders with the enemy layer and a Lockable component. Then, compute the distance to the center of the screen and check if the target is visible (linecast between player and target).


In ```Update()```, we check if the target remains at a certain distance and on sight, and unlock if it doesn't. We can apply a certain tolerance so that it does not unlock the target instantly, but after N seconds.

Additionally, if the player moves the right stick to the left or right, a new scan is triggered to find a new target either left or right of the current one, respectively. To smooth the transition to the new target, it is useful to set a generous deadzone in the Cinemachine camera. This way, as the new target is probably going to be in the deadzone anyway (because else it would detect it), the camera will try to center to it smoothly. If no deadzone is set, the transition is a cut (very dizzy and confusing).

### Camera behavior

When the player is not locked on, we use a Cinemachine Free Look camera, with three orbital rigs that allows us to control, to some extent, the visual field. In this set-up, the camera **follows** and **looks at** the player. 

![Free look camera](https://github.com/CesarCaramazana/Unity/blob/main/Camera/Lock%20on%20system/Images/FreeLook%20(Follow%20player)%20camera.PNG)

We set up a Cinemachine Virtual camera that **follows** the player and **looks at** the target. 

![Virtual Camera setup](https://github.com/CesarCaramazana/Unity/blob/main/Camera/Lock%20on%20system/Images/Virtual%20(LockOn)%20camera.PNG)

*Note: since the player is not in control of this camera, the Input Provider does not have a reference to the Input Action*.

When the target has been selected, we increase the ```Priority``` of the Lock-on Camera with respect to our Free Look camera (unlocked). 

```
lockCamera.m_LookAt = lockTarget;
lockCamera.Priority = 11;
lockCamera.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.y = lockYOffset;
```
Another way to transition between cameras --and probably a better way, since this is not very smooth--, is to use animation states (Cinemachine State Driven Camera). A problem I encountered with my set up is that switching back to the follow camera leads to sudden changes in the position and rotation, which may cause motion sickness. This is due to the Free Look camera not updating while the Lock On camera is on. 

My temporary solution to this problem is to enable ```Target recentering``` in the FreeLook camera, if only for a few frames, so that at least the forward directions of both cameras are aligned. This is not, by all means, the ideal solution.

### UI reticle
When locked on, a raycast is thrown from the player to the target. We detect the hit point on the enemy, offset the distance a little bit towards the player to avoid Z-fighting, and place a sprite renderer that always looks at the camera.

```
Vector3 direction = ((lockTarget.position + Vector3.up * lockYOffset) - mainCamera.position).normalized;

if(Physics.Linecast(mainCamera.position, lockTarget.position + Vector3.up * lockYOffset, out RaycastHit hit, enemyLayer))
{
     reticle.transform.position = hit.point - direction * 0.2f;
     reticle.transform.LookAt(mainCamera.position);
}      
```

An example:

![Reticle](https://github.com/CesarCaramazana/Unity/blob/main/Camera/Lock%20on%20system/Images/Reticle.PNG)


### Player movement
We have to communicate the Player Controller that we are locked on. With this boolean, we can modify the movement of the player, for example, by restricting its rotation and playing a different set of animations. 

Problems:
- If the player is looking at the enemy and jumps, the rotation in X and Z will become greater than zero. If the player unlocks on the enemy while on air, this values will not automatically return to zero, causing it to face the floor. So it has to be done manually.

# Lock-on System with Cinemachine

The implementation of this lock on system builds upon the tutorial by [Game Factory](https://www.youtube.com/watch?v=ozb6aFTwiHU&ab_channel=GameFactory), with a few modifications and improvements that better suit my case scenario.


## 
A lock-on system is a mechanic that allows the camera to focus on a single target, usually so that it remains at the center of the screen in 3rd person games, taking control of the camera away from the player. Here we are refering to a **camera behavior**, rather than a soft-lock system of player magnetism (such as the one in the Batman Arkham trilogy). 
Some examples of games that have a camera lock-on system similar to what we are trying to accomplish: Dark Souls, God of War 2018 or Nier Automata.

The main problematiques that have to be addressed in a lock-on system are:
- How to detect nearby enemies (potential targets).
- How to select the target among all possibilities.
- How to set up the camera to focus on the target.
- How to provide visual feedback on the target selected (UI reticle).
- How the player movement is modified when locked on.



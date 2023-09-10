# An Audio Event implementation with Scriptable Objects

The [original implementation by Baruch-Adi Hen](https://gist.github.com/baruchadi/3c23caf609fa0f4bd349d9ea432eb9c4).

Here I present a simplified version, with a custom Unity editor to create the preview button in the inspector, and support for Mixer Group selection.


##

Scriptable objects allow us to create audio assets that do not need to be attached to game objects. Additionally, this implementation lets us create lists of audio clips to be played during runtime, either by order or randomly, which is a nice way of providing variety to the audio system.

An example of audio event:

![Audio Event](https://github.com/CesarCaramazana/Unity/blob/main/Audio/AudioScriptableObjects/Images/AudioEvent_sfxExample.PNG)

Pros:
- Audio variety through lists of clips.
- Audio events are created as assets.


This implementation, however, has some shortcomings:
- Audio sources are created and destroyed each time we want to play an audio, which is not efficient.
- There is no Stop() function to interrupt a particular sources once it has been triggered.

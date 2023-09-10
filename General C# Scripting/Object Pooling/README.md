# Object Pooling

Some references for the pools' implementation in [bendux](https://www.youtube.com/watch?v=YCHJwnmUGDk&ab_channel=bendux)'s channel, [Brackeys](https://www.youtube.com/watch?v=tdSmKaJvCoA&t=906s&ab_channel=Brackeys) and [Sebastian Lague](https://www.youtube.com/watch?v=LhqP3EghQ-Q&ab_channel=SebastianLague).

The ObjectPooling.cs script is not generic because I used an enum ProjectileType to create the dictionary that saves the different pools (for bullet pooling). However, we could use a string as reference and use it for any kind of object.
The ObjectPooling.cs implementation is more powerful in the sense that we can pool more than one object.


## What is object pooling

Instead of creating and destroying objects at runtime with the Instantiate() and Destroy() methods from MonoBehavior, we can instead have them inactive in the hierarchy from the beginning and enable/disable them at need. This is more efficient when objects have to be created and destroyed very quickly during gameplay, such as in a bullet hell game, since the garbage collector (that cleans up the memory of destroyed objects) can cause some framedrops.

The key difference is that, whenever we would Instantiate() the object, we have to instead call the GetObjectFromPool() method, enable it (SetActive()) and position it where we want it (for example, in the muzzle of the gun). Instead of Destroy(), we just disable it.

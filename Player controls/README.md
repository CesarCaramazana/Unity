# Player controller

There are two main approaches to player controllers in Unity: Rigidbody-based and Character controller-based.


## Rigidbody (physics)

Using Unity's physics system and applying forces to rigidbody.

Pros:
- Making use of Physics allows for interaction between rigidbodies, such as pushing an object.
- Realistic movement.
- Allows to use Unity's physics options for some actions (such as grappling with spring joints). 

Cons:
- Physics can be tricky and unexpected behavior may emerge.
- Mixing Update() and FixedUpdate().
- Physics can be computationally expensive.
- Coding direction: from realism to *precise controls*.


## Character controller

Using the Character controller component, which allows for custom implementation of everything.

Pros:
- Max control over the options, ideal for gameplay that requires precision and snappy controls.
- Straightforward implementation.

Cons:
- You have to implement everything from scratch, such as gravity.
- Coding direction: from accurate controls to *fluid controls*.

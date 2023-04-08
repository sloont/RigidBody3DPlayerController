# RigidBody3DPlayerController
RigidBody3D Player Controller for Godot 4 written in C#

## useful godot source links
godot_body_3d.cpp
https://github.com/godotengine/godot/blob/44d539465acca7592e0c88748e231fe5f151da37/servers/physics_3d/godot_body_3d.cpp#L475
godot_body_3d.h
https://github.com/godotengine/godot/blob/61630d4e1e279278d29576b979b710b9025f473e/servers/physics_3d/godot_body_3d.h#L254

built this to replicate and isolate an issue with a private project

## changelog
  
#### 12fae100f4254b23a4a603c4e1c8d5181a49d4f3
- no longer manually setting LinearVelocity
- Player scene mass is 80kg
- Ground scene physics material has friction 0.75
- lock rotation removed in favor of locking each angular axis
- stutter while in the air is gone
- Walking.cs now uses ac/deceleration values about 20x greater (hard-coded hack)
- Jumping.cs `JumpVelocity` changed to `JumpAcceleration`. Calculation involves dividing by `delta`
- `_PhysicsProcess` callback now includes an `ApplyCentralForce` call with `Mass` x calculated "velocity"
- `_Move` method lerps Y-velocity in the down direction with a weight of Gravity * `delta`

#### issues
- gravity is hard-coded because i was lazy
- not satisfied with how it feels
- is it even physics? velocity is used when maybe certain things are not actually velocities
- iteration of the different `Movement.CalculateLinearVelocity` needs to be reimagined.
  this is effectively "desired velocity"
- specific calculations for `Walking` and `Jumping` are hacks
- jump floats forever

#### 3414af8ed4994341588217d2881814a02edd1e23
- core functionality/init
- meant to be 1:1 with another project
- goal is to develop a concise controller for physics based movement controls

#### issues
- long story short, this flawed controller makes it obvious that physics is not being respected
- manually setting velocity within the `_IntegrateForces` callback creates an interesting vibrational effect
  when jumping/falling.

  current theory is gravity being applied as a force while linearVelocity.Y (and X,Z) is applied as a velocity.
  likely that not using mass to calculate these values is causing the 'shake'

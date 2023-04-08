# RigidBody3DPlayerController
RigidBody3D Player Controller for Godot 4 written in C#

https://github.com/godotengine/godot/blob/44d539465acca7592e0c88748e231fe5f151da37/servers/physics_3d/godot_body_3d.cpp#L475

## issues
built this to replicate and isolate an issue with a private project
- long story short, this flawed controller makes it obvious that physics is not being respected
- manually setting velocity within the `_IntegrateForces` callback creates an interesting vibrational effect
  when jumping/falling.

  current theory is gravity being applied as a force while linearVelocity.Y (and X,Z) is applied as a velocity.
  likely that not using mass to calculate these values is causing the 'shake'

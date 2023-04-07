using Godot;

public partial class Walking : Movement
{
	[Export] public float acceleration = 8f;
	[Export] public float deceleration = 8f;

	[Export] public float airborneDamping = 0.3f;

	public override bool ShouldActivate()
	{
		return true;
	}

	public override Vector3 CalculateLinearVelocity(
		Vector3 direction,
		Vector3 linearVelocity,
		float speed,
		bool grounded,
		double delta
	)
	{
		// if we're not using this Movement, return the linearVelocity we were given
		if (!isMovementOn) {
			return linearVelocity;
		}
		GD.Print("Movement ON");

		// Walking is only concerned with 2D velocity: X and Z
		Vector3 tempVelocity = linearVelocity;
		tempVelocity.Y = 0;

		// Save temp acceleration for mutation of param acceleration
		float tempAcceleration;
		// Need to know where we're going for Lerp
		Vector3 destination = direction * speed;

		// Use dot product of destination and velocity to find out whether we should be
		// accelerating or decelerating
		tempAcceleration = (direction.Dot(tempVelocity) > 0) ? acceleration : deceleration;

		// Don't care about LinearVelocity.Y, but need to know if we're in the air
		if (!grounded) {
			tempAcceleration *= airborneDamping;
		}

		// Lerp LinearVelocity to destination using acceleration * time
		tempVelocity = tempVelocity.Lerp(destination, tempAcceleration * (float)delta);

		// Save the new X and Z values to the linearVelocity param we were passed.
		linearVelocity.X = tempVelocity.X;
		linearVelocity.Z = tempVelocity.Z;

		return linearVelocity;
	}
}

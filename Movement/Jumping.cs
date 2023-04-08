using Godot;

public partial class Jumping : Movement
{
	[Export] public float JumpAcceleration = 10f;

	public override bool ShouldActivate()
	{
		return (
			_player.receivedJumpInput &&
			_player.grounded
		);
	}

	public override Vector3 CalculateLinearVelocity(
		Vector3 direction,
		Vector3 linearVelocity,
		float speed,
		bool grounded,
		double delta
	)
	{
		// If velocity is m/s
		//    force is kg*m/s*s
		//    impulse is kg*m/s

		// if we're jumping. Set linearVelocity.Y to jumpVelocity
		if (isMovementOn) {
			linearVelocity.Y = JumpAcceleration / (float)delta;
			GD.Print("Movement ON");
		}

		return linearVelocity;
	}
}

using Godot;

public partial class Jumping : Movement
{
	[Export] public float JumpVelocity = 9f;

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
		// if we're jumping. Set linearVelocity.Y to jumpVelocity
		if (isMovementOn) {
			linearVelocity.Y = JumpVelocity;
			GD.Print("Movement ON");
		}

		return linearVelocity;
	}
}

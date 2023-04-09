using Godot;

public partial class Jumping : Movement
{
	[Export] public float JumpVelocity = 5f;

	public override bool ShouldActivate()
	{
		return (
			_player.receivedJumpInput &&
			_player.grounded
		);
	}

	public override bool CalculateLinearVelocity(
		Vector3 direction,
		ref Vector3 linearVelocity,
		bool grounded
	)
	{
		// if we just jumped. Set linearVelocity.Y to jumpVelocity
		if (isMovementOn) {
			linearVelocity.Y = JumpVelocity;
			return true;
		}

		return false;
	}
}

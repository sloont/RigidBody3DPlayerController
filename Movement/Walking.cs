using Godot;

public partial class Walking : Movement
{
	[Export] public float positiveVelocityCoeffecient = 0.2f;
	[Export] public float negativeVelocityCoefficient = 0.1f;

	[Export] public float airborneDamping = 0.3f;

	public override bool ShouldActivate()
	{
		// temporary
		return true;
	}

	public override bool CalculateLinearVelocity(
		Vector3 direction,
		ref Vector3 linearVelocity,
		bool grounded
	)
	{
		if (!isMovementOn) {
			return false;
		}

		// Use dot product of destination and velocity to find out whether we should be
		// accelerating or decelerating
		float velocityCoefficient = (direction.Dot(linearVelocity) > 0) ? positiveVelocityCoeffecient : negativeVelocityCoefficient;

		Vector3 commandVelocity = direction * velocityCoefficient;

		if (!grounded) {
			commandVelocity *= airborneDamping;
		}

		linearVelocity += commandVelocity;
		linearVelocity = linearVelocity.Normalized() * Mathf.Clamp(linearVelocity.Length(), 0, 10f);

		return true;
	}
}

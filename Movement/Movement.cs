using Godot;

public abstract partial class Movement : Node3D
{
	[Signal] public delegate void MovementOnEventHandler();
	[Signal] public delegate void MovementOffEventHandler();

	private bool _isMovementOn = false;
	private bool _wasMovementOn = false;
	[Export] public bool isMovementOn {
		get {
			return _isMovementOn;
		}
		set {
			_wasMovementOn = _isMovementOn;
			_isMovementOn = value;

			if (_wasMovementOn == _isMovementOn) {
				return;
			}

			if (_isMovementOn) {
				EmitSignal(Movement.SignalName.MovementOn);
			}
			else {
				EmitSignal(Movement.SignalName.MovementOff);
			}
		}
	}

	protected Player _player;

	public override void _Ready()
	{
		_player = GetParent<Player>();
	}

	// these should be specific and customized in the child class
	public virtual bool ShouldActivate()
	{
		return false;
	}

	public abstract bool CalculateLinearVelocity(
		Vector3 direction,
		ref Vector3 linearVelocity,
		bool grounded
	);

	public void PrintClass(Variant custom)
	{
		GD.Print($"{GetType()} calculateLinearVelocity called.");
		GD.Print($"Custom data: {custom.ToString()}");
	}
}

using Godot;
using System;

public partial class Player : RigidBody3D
{
	// save input map strings here as constants
	public const string INPUT_MOVE_FORWARD = "move_forward";
	public const string INPUT_MOVE_BACKWARD = "move_backward";
	public const string INPUT_MOVE_LEFT = "move_left";
	public const string INPUT_MOVE_RIGHT = "move_right";

	// Vector2 for horizontal direction based on inputs
	private Vector2 _inputMoveDirection = Vector2.Zero;

	// All movement
	[Export] public Godot.Collections.Array<String> movementComponentsPaths = new Godot.Collections.Array<String>();
	public Godot.Collections.Array<NodePath> movementNodePaths = new Godot.Collections.Array<NodePath>();
	public Godot.Collections.Array<Movement> movementComponents = new Godot.Collections.Array<Movement>();

	// movement nodes
	private Jumping _jumpingComponent;
	private Walking _walkingComponent;

	// floor detection shapecast
	private ShapeCast3D _floorDetection;

	// bools for checking airborne status
	private bool _grounded = true;
	public bool grounded {
		get {
			return _grounded;
		}
		private set {
			_grounded = value;
		}
	}
	private bool _wasGrounded = true;

	// store jump input when we receive it. reset at end of tick
	private bool _receivedJumpInput = false;
	public bool receivedJumpInput {
		get {
			return _receivedJumpInput;
		}
		private set {
			_receivedJumpInput = value;
		}
	}

	// bools for checking walking status
	private bool _walking = false;
	private bool _wasWalking = false;

	// player speed
	[Export]
	public float speed = 10f;

	// storage for velocities
	private Vector3 _linearVelocity = Vector3.Zero;
	private Vector3 _lastLinearVelocity = Vector3.Zero;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_CacheGetNodes();
		_LoadMovementComponents();
	}

	public override void _Input(InputEvent @event)
	{
		return;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	// Using Physics Process to calculate the desired velocity force as a function of engine step time (delta)
	public override void _PhysicsProcess(double delta)
	{

		_inputMoveDirection = Input.GetVector(INPUT_MOVE_LEFT, INPUT_MOVE_RIGHT, INPUT_MOVE_FORWARD, INPUT_MOVE_BACKWARD);

		if (Input.IsActionJustPressed("jump")) {
			Jump();
			GD.Print("Jump input");
			GD.Print(receivedJumpInput);
		}

		_lastLinearVelocity = _linearVelocity;

		_linearVelocity = _Move(delta);
	}

	// _Move method is provided engine physics delta
	private Vector3 _Move(double delta)
	{
		// transform 2D inputs to 3D space
		Vector3 moveDirection = _ProcessInputMoveDirection(_inputMoveDirection);

		// store current LinearVelocity in tempVelocity for processing
		Vector3 tempVelocity = LinearVelocity;

		// set grounded bools
		_CheckGrounded();

		// iterate through movement components and check if they should activate
		foreach (Movement movement in movementComponents) {
			movement.isMovementOn = movement.ShouldActivate();

			tempVelocity = movement.CalculateLinearVelocity(
				moveDirection,
				tempVelocity,
				speed,
				_grounded,
				delta
			);

			// GD.Print(movement.Name, ":  ", tempVelocity);
		}

		_CheckWalking(tempVelocity);

		// reset the jump bool. we are finished with it.
		_receivedJumpInput = false;

		return tempVelocity;



	}

	// safe space for rigid body state modification
	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		state.LinearVelocity = _linearVelocity;

		// GD.Print(state.LinearVelocity);
	}

	public void Jump()
	{
		receivedJumpInput = true;
	}

	// extracted into its own method for easier animations
	private bool _CheckGrounded()
	{
		grounded = _floorDetection.IsColliding();

		if (!_wasGrounded && _grounded) {
			// EMIT SIGNAL
		}

		return _wasGrounded = _grounded;
	}

	// extracted into its own method for easier animations
	private bool _CheckWalking(Vector3 velocity)
	{
		velocity.Y = 0;

		_walking = grounded && Mathf.Abs(_inputMoveDirection.Length()) > 0.1;

		if (!_wasWalking && _walking) {
			// EMIT SIGNAL
		}

		if (_wasWalking && !_walking) {
			// EMIT SIGNAL
		}

		return _wasWalking = _walking;
	}

	private void _LoadMovementComponents()
	{
		foreach (String movementComponentPath in movementComponentsPaths)
		{
			PackedScene newComponentScene =  ResourceLoader.Load<PackedScene>(movementComponentPath);
			Movement newComponent = newComponentScene.Instantiate<Movement>();
			AddChild(newComponent);
			movementNodePaths.Add(GetPathTo(newComponent));
			movementComponents.Add(newComponent);
		}

		//TODO: this can probably be read from a config instead of hard-coded
		_jumpingComponent = (Jumping)_GetMovementComponent("Jumping");
		_walkingComponent = (Walking)_GetMovementComponent("Walking");
	}

	private Movement _GetMovementComponent(string movementName)
	{
		foreach (NodePath path in movementNodePaths)
		{
			Movement movementComponent = GetNode<Movement>(path);
			if (movementComponent.Name == movementName)
			{
				return movementComponent;
			}
		}

		return null;
	}

	// helper to convert input 2D movement vector to 3D
	// multiplied by Transform.Basis and normalized
	private Vector3 _ProcessInputMoveDirection(Vector2 inputMoveDirection)
	{
		return (Transform.Basis * new Vector3(inputMoveDirection.X, 0, inputMoveDirection.Y)).Normalized();
	}

	///

	// basically @onready nodes
	private void _CacheGetNodes()
	{
		_floorDetection = GetNode<ShapeCast3D>("%FloorDetection");
	}
}

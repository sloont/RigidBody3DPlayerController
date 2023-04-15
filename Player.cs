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
	// camera pivot
	private Node3D _pivot;

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
	private Vector3 _commandLinearVelocity = Vector3.Zero;
	private Vector3 _lastLinearVelocity = Vector3.Zero;

	public float Gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

	// rotation
	private Vector2 _inputLookDirection = Vector2.Zero;
	private Vector3 _playerRotation = Vector3.Zero;
	private Vector3 _cameraRotation = new Vector3(-0.52f,0,0);

	//
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_CacheGetNodes();
		_LoadMovementComponents();
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion inputEvent) {
			_inputLookDirection = inputEvent.Relative * 0.01f;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	// Using Physics Process to calculate the desired velocity force as a function of engine step time (delta)
	public override void _PhysicsProcess(double delta)
	{

		_inputMoveDirection = Input.GetVector(INPUT_MOVE_LEFT, INPUT_MOVE_RIGHT, INPUT_MOVE_FORWARD, INPUT_MOVE_BACKWARD);
		_RotatePlayer(delta);
		if (Input.IsActionJustPressed("jump")) {
			Jump();
		}

		_lastLinearVelocity = LinearVelocity;

		_UpdateCommandLinearVelocity();
		_ApplyCustomFriction();

		Vector3 commandAcceleration = (_commandLinearVelocity - _lastLinearVelocity) / (float)delta;

		ApplyCentralForce(commandAcceleration * Mass);

		// reset the jump bool. we are finished with it.
		_receivedJumpInput = false;
	}

	// _Move method is provided engine physics delta
	private void _UpdateCommandLinearVelocity()
	{
		// transform 2D inputs to 3D space
		Vector3 moveDirection = _ProcessInputMoveDirection(_inputMoveDirection);

		// store current LinearVelocity in _commandLinearVelocity for processing
		_commandLinearVelocity = LinearVelocity;

		// set grounded bools
		_CheckGrounded();

		// iterate through movement components and check if they should activate
		foreach (Movement movement in movementComponents) {
			movement.isMovementOn = movement.ShouldActivate();

			movement.CalculateLinearVelocity(
				moveDirection,
				ref _commandLinearVelocity,
				_grounded
			);
		}

		_CheckWalking();
	}

	// safe space for rigid body state modification
	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		GD.Print("Speed: ", state.LinearVelocity.Length());
		_debugDrawVector3(state.LinearVelocity.Normalized(), new Color(1,1,0,1));
	}
		private void _ApplyCustomFriction() {
		float slopeAngle = 0;
		Vector3 up = Vector3.Up; // direction of gravity
		Quaternion rotateY = new(0,0,0,1);

		if (_floorDetection.IsColliding()) {
			Vector3 groundNormal = _floorDetection.GetCollisionNormal(0);
			slopeAngle = Mathf.RadToDeg(groundNormal.AngleTo(Vector3.Up));
			rotateY = new Quaternion(groundNormal, Vector3.Up);

			if (slopeAngle != 0 && slopeAngle < 45.0f) {
				// Vector3 customFriction = Vector3.Forward * temp * -Gravity * Mathf.Sin(slopeAngle) * Mass;
				// groundNormal should NOT be rotated by Transform.Basis
				up = groundNormal;
				Vector3 frictionDirection = new Vector3(-up.X, 0, -up.Z) * rotateY;
				// float coefficient = 1 + (Mathf.Sqrt(Mathf.Abs(Mathf.Cos(slopeAngle))) - Mathf.Sqrt(Mathf.Abs(Mathf.Sin(slopeAngle))));
				float coefficient = (Mathf.Cos(slopeAngle));
				float subcoefficient = 1/(19.1f*coefficient);

				// _debugDrawVector3(new Vector3(up.X, 0, 0), new Color(1,0,0,1));
				// _debugDrawVector3(new Vector3(0, up.Y, 0), new Color(0,1,0,1));
				// _debugDrawVector3(new Vector3(0, 0, up.Z), new Color(0,0,1,1));
				_debugDrawVector3(frictionDirection, new Color(0,1,1,1));
				_debugDrawVector3(up, new Color(1,0,1,1));

				ApplyCentralForce(frictionDirection * Gravity * GravityScale * Mass * (coefficient + subcoefficient));

				GD.Print(
					"slope angle: ", slopeAngle, "deg",
					"\nup Vector3: ", up,
					"\ncoefficient: ", coefficient,
					"\nsubCoefficient: ", subcoefficient,
					"\n"
				);
			}

		}

		// ApplyCentralForce(); //*
		// _debugDrawVector3(Vector3.Up, new Color(0,1,1,1));
	}

	private void _debugDrawVector3(Vector3 vector, Color color)
	{
		MeshInstance3D debugMesh = new();
		ImmediateMesh immediateMesh = new();

		debugMesh.Mesh = immediateMesh;

		StandardMaterial3D debugMaterial = new StandardMaterial3D();

		immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, debugMaterial);
		immediateMesh.SurfaceAddVertex(GlobalPosition);
		immediateMesh.SurfaceAddVertex(GlobalPosition + vector.Normalized());
		immediateMesh.SurfaceEnd();
		debugMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		debugMaterial.AlbedoColor = color;

		GetTree().Root.AddChild(debugMesh);

		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(debugMaterial, "albedo_color", new Color(color.R,color.G,color.B,0), 1.0);
		tween.TweenCallback(Callable.From(debugMesh.QueueFree));
	}

	private void _RotatePlayer(double delta) {

		float mouseSensitivity = 5f;

		// feed Input our 4 strings (-x, x, -y, y) and return a Vector2
		_inputLookDirection += Input.GetVector("look_left", "look_right", "look_up", "look_down");

		//structs have to be set to a Vector3 variable instead of direct modification
		// set camera rotation y
		_playerRotation.Y = this.Rotation.Y - _inputLookDirection.X * mouseSensitivity * (float) delta;

		_cameraRotation.X += this.Rotation.X - _inputLookDirection.Y * mouseSensitivity * (float) delta;

		_cameraRotation.X = Math.Max(Math.Min(1.57f, _cameraRotation.X), -1.57f); // Clamp?
		// assign these values to our Camera
		_pivot.Rotation = _cameraRotation;
		// assign these values to our Player
		this.Rotation = _playerRotation;
		// reset look direction vector to zero
		_inputLookDirection = Vector2.Zero;
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
	private bool _CheckWalking()
	{
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
		_pivot = GetNode<Node3D>("%Pivot");
	}
}

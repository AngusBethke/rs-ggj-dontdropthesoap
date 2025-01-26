using DontDropTheSoap.Helpers;
using Godot;
using System;

public partial class Player : RigidBody3D
{
	private float _mouseSensitivity = 0.001f;
	private float _twistInput = 0.0f;
	private float _pitchInput = 0.0f;

	protected Node3D Child_TwistPivot => GetNode<Node3D>("TwistPivot");
    protected Node3D Child_PitchPivot => Child_TwistPivot.GetNode<Node3D>("PitchPivot");
    protected MeshInstance3D Child_Vehicle => GetNode<MeshInstance3D>("Vehicle");
    protected CollisionShape3D Child_VehileCollision => GetNode<CollisionShape3D>("VehicleCollision");
    protected RayCast3D Child_RayCast => Child_VehileCollision.GetNode<RayCast3D>("RayCast3D");
    protected GpuParticles3D Child_Particles => Child_Vehicle.GetNode<GpuParticles3D>("VehicleParticles");


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured; 
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ManagePlayerMovement((float)delta);

        if (Input.IsActionJustPressed(UIHelperConstants.EscapeMenuOpened))
		{
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        ManageCameraRotation();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
		if (@event is InputEventMouseMotion mouseMotionEvent)
		{
			if (Input.MouseMode == Input.MouseModeEnum.Captured)
			{
				_twistInput = -(mouseMotionEvent.Relative.X * _mouseSensitivity);
				_pitchInput = -(mouseMotionEvent.Relative.Y * _mouseSensitivity);
			}
		}
    }

	private void ManagePlayerMovement(float delta)
	{
        var input = Vector3.Zero;
        input.X = Input.GetAxis(UIHelperConstants.PlayerLeft, UIHelperConstants.PlayerRight);
        input.Z = Input.GetAxis(UIHelperConstants.PlayerForward, UIHelperConstants.PlayerBack);

        EnsurePlayerCannotExceedMapBounds();

        var twistPivotInput = Child_TwistPivot.Basis * input;

        if (Input.IsActionJustPressed(UIHelperConstants.PlayerJump))
        {
            if (CanJump())
            {
                twistPivotInput.Y += 40;
            }
        }

        ApplyCentralForce(twistPivotInput.UniformMultiply(1200 * delta));

        if (LinearVelocity.X > 0 || LinearVelocity.Y > 0 || LinearVelocity.Z > 0)
        {
            Child_Particles.Emitting = true;
        }
        else
        {
            Child_Particles.Emitting = false;
        }
    }

    private void EnsurePlayerCannotExceedMapBounds()
    {
        // Ensure the player character cannot escape map bounds
        var capturedVector = new Vector3(Position.X, Position.Y, Position.Z);
        var debugValue = Position.PositionClampToMapBounds();

        if (capturedVector != debugValue)
        {
            Position = debugValue;
        }
    }

	private void ManageCameraRotation()
	{
        Child_TwistPivot.RotateY(_twistInput);

        if (Child_Vehicle != null)
        {
            Child_Vehicle.RotateY(_twistInput);
        }

        if (Child_VehileCollision != null)
        {
            Child_VehileCollision.RotateY(_twistInput);
        }

        Child_PitchPivot.RotateX(_pitchInput);
        Child_PitchPivot.Rotation = Child_PitchPivot.Rotation.Clamp(Vector3Fields.X, 30, 5);

        _twistInput = 0;
        _pitchInput = 0;
    }

    private bool CanJump()
    {
        if (Child_RayCast.IsColliding())
        {
            return true;
        }

        return false;
    }
}

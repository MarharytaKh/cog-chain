using Godot;

public partial class OrbitCamera : Node3D
{
	[Export] public Vector3 Target = Vector3.Zero;
	[Export] public float Distance = 8f;
	[Export] public float RotateSpeed = 0.3f;
	[Export] public float MinPitch = -10f;
	[Export] public float MaxPitch = 60f;
	[Export] public float MinYaw = -60f;
	[Export] public float MaxYaw = 60f;

	private float _yaw = 0f;
	private float _pitch = 20f;
	private Vector2 _lastTouch = Vector2.Zero;
	private bool _touching = false;
	private int _touchIndex = -1;

	public override void _Ready()
	{
		UpdateCamera();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventScreenTouch touch)
		{
			if (touch.Pressed && _touchIndex == -1)
			{
				_touchIndex = touch.Index;
				_touching = true;
				_lastTouch = touch.Position;
			}
			else if (!touch.Pressed && touch.Index == _touchIndex)
			{
				_touching = false;
				_touchIndex = -1;
			}
		}

		if (@event is InputEventScreenDrag drag && _touching && drag.Index == _touchIndex)
		{
			Vector2 delta = drag.Position - _lastTouch;
			_lastTouch = drag.Position;
			_yaw -= delta.X * RotateSpeed;
			_yaw = Mathf.Clamp(_yaw, MinYaw, MaxYaw);
			_pitch -= delta.Y * RotateSpeed;
			_pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);
			UpdateCamera();
		}

		if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Right)
		{
			_touching = mb.Pressed;
			if (mb.Pressed)
				_lastTouch = mb.Position;
		}

		if (@event is InputEventMouseMotion mm && _touching)
		{
			_yaw -= mm.Relative.X * RotateSpeed;
			_yaw = Mathf.Clamp(_yaw, MinYaw, MaxYaw);
			_pitch += mm.Relative.Y * RotateSpeed;
			_pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);
			UpdateCamera();
		}
	}

	private void UpdateCamera()
	{
		float yawRad = Mathf.DegToRad(_yaw);
		float pitchRad = Mathf.DegToRad(_pitch);
		Vector3 offset = new Vector3(
			Distance * Mathf.Cos(pitchRad) * Mathf.Sin(yawRad),
			Distance * Mathf.Sin(pitchRad),
			Distance * Mathf.Cos(pitchRad) * Mathf.Cos(yawRad)
		);
		GlobalPosition = Target + offset;
		LookAt(Target, Vector3.Up);
	}
}

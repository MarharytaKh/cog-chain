using Godot;
public partial class LevelAnimation : Node3D
{
	public enum AnimationType
	{
		Door,
		Arrow,
		Lamp,
		Rotate,
		Lift,
		ClockHand,
		Key
	}

	[Export] public AnimationType Type = AnimationType.Lamp;
	[Export] public float Duration = 1.0f;
	[Export] public float TargetAngle = 45.0f;
	[Export] public float RotateSpeed = 2.0f;
	[Export] public float EmissionTarget = 5.0f;
	[Export] public string MeshNodeName = "MeshInstance3D";
	[Export] public int TotalSteps = 1; // для Door: сколько таргетов открывают
	[Export] public float LiftHeight = 3.0f; // для Lift: высота подъёма
	[Export] public float ClockSpeed = 1.0f; // для ClockHand: скорость вращения

	private int _currentStep = 0;
	private float _currentTargetAngle = 0f;
	private bool _activated = false;
	private float _timer = 0f;
	private Vector3 _startRotation;
	private Vector3 _startPosition;
	private MeshInstance3D _mesh;
	private StandardMaterial3D _material;
	private OmniLight3D _light;

	public override void _Ready()
	{
		_startRotation = RotationDegrees;
		_startPosition = Position;

		if (Type == AnimationType.Lamp)
		{
			_mesh = GetNodeOrNull<MeshInstance3D>(MeshNodeName);
			if (_mesh != null)
			{
				var mat = _mesh.GetActiveMaterial(0)?.Duplicate() as StandardMaterial3D;
				if (mat != null)
				{
					_mesh.SetSurfaceOverrideMaterial(0, mat);
					mat.EmissionEnabled = true;
					mat.EmissionEnergyMultiplier = 0f;
					_material = mat;
				}
			}
			_light = GetNodeOrNull<OmniLight3D>("OmniLight3D");
			if (_light != null) _light.Visible = false;
		}
	}

public void Activate()
{
	if (_currentStep >= TotalSteps) return; // уже полностью активирован
	_currentStep++;
	_activated = true;
	_timer = 0f;

	if (Type == AnimationType.Door)
		_currentTargetAngle = TargetAngle * ((float)_currentStep / TotalSteps);
}

	public override void _Process(double delta)
	{
		if (!_activated) return;

		switch (Type)
		{
			case AnimationType.Door:
				_timer += (float)delta;
				float tDoor = Mathf.Clamp(_timer / Duration, 0f, 1f);
				float easeDoor = 1f - Mathf.Pow(1f - tDoor, 3f);
				RotationDegrees = new Vector3(
					_startRotation.X,
					_startRotation.Y,
					_startRotation.Z + _currentTargetAngle * easeDoor
				);
				break;

			case AnimationType.Arrow:
				_timer += (float)delta;
				float tArrow = Mathf.Clamp(_timer / Duration, 0f, 1f);
				RotationDegrees = new Vector3(
					_startRotation.X,
					_startRotation.Y + TargetAngle * tArrow,
					_startRotation.Z
				);
				break;

			case AnimationType.Lamp:
				_timer += (float)delta;
				float tLamp = Mathf.Clamp(_timer / Duration, 0f, 1f);
				float easeLamp = 1f - Mathf.Pow(1f - tLamp, 3f);
				if (_material != null)
					_material.EmissionEnergyMultiplier = EmissionTarget * easeLamp;
				if (_light != null && !_light.Visible && easeLamp > 0.1f)
					_light.Visible = true;
				break;

			case AnimationType.Rotate:
				_timer += (float)delta;
				RotationDegrees = new Vector3(
					_startRotation.X,
					_startRotation.Y + RotateSpeed * _timer * Mathf.RadToDeg(1f),
					_startRotation.Z
				);
				break;

			case AnimationType.Lift:
				_timer += (float)delta;
				float tLift = Mathf.Clamp(_timer / Duration, 0f, 1f);
				float easeLift = 1f - Mathf.Pow(1f - tLift, 3f);
				Position = new Vector3(
					_startPosition.X,
					_startPosition.Y + LiftHeight * easeLift,
					_startPosition.Z
				);
				break;

			case AnimationType.ClockHand:
				_timer += (float)delta;
				RotationDegrees = new Vector3(
					_startRotation.X,
					_startRotation.Y,
					_startRotation.Z + ClockSpeed * _timer * Mathf.RadToDeg(1f)
				);
				break;
			case AnimationType.Key:
	_timer += (float)delta;
	float tKey = Mathf.Clamp(_timer / Duration, 0f, 1f);
	float easeKey = 1f - Mathf.Pow(1f - tKey, 3f);
	RotationDegrees = new Vector3(
		_startRotation.X + TargetAngle * easeKey,
		_startRotation.Y,
		_startRotation.Z
	);
	break;
		}
	}
}

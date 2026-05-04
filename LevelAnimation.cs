using Godot;
public partial class LevelAnimation : Node3D
{
	public enum AnimationType
	{
		Door,
		Arrow,
		Lamp,
		Rotate
	}
	[Export] public AnimationType Type = AnimationType.Lamp;
	[Export] public float Duration = 1.0f;
	[Export] public float TargetAngle = 45.0f;
	[Export] public float RotateSpeed = 2.0f;
	[Export] public float EmissionTarget = 5.0f;
	[Export] public string MeshNodeName = "MeshInstance3D";

	private bool _activated = false;
	private float _timer = 0f;
	private Vector3 _startRotation;
	private MeshInstance3D _mesh;
	private StandardMaterial3D _material;
	private OmniLight3D _light;

	public override void _Ready()
	{
		_startRotation = RotationDegrees;
		if (Type == AnimationType.Lamp)
		{
			_mesh = GetNodeOrNull<MeshInstance3D>(MeshNodeName);
			GD.Print("Lamp mesh найден: " + (_mesh != null));
			if (_mesh != null)
			{
				var mat = _mesh.GetActiveMaterial(0)?.Duplicate() as StandardMaterial3D;
				GD.Print("Material найден: " + (mat != null));
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
		_activated = true;
		_timer = 0f;
	}

	public override void _Process(double delta)
	{
		if (!_activated) return;
		_timer += (float)delta;
		float t = Mathf.Clamp(_timer / Duration, 0f, 1f);
		float ease = 1f - Mathf.Pow(1f - t, 3f);
		switch (Type)
		{
			case AnimationType.Door:
				RotationDegrees = new Vector3(
					_startRotation.X,
					_startRotation.Y,
					_startRotation.Z + TargetAngle * ease
				);
				break;
			case AnimationType.Arrow:
				RotationDegrees = new Vector3(
					_startRotation.X,
					_startRotation.Y + TargetAngle * t,
					_startRotation.Z
				);
				break;
			case AnimationType.Lamp:
				if (_material != null)
					_material.EmissionEnergyMultiplier = EmissionTarget * ease;
				if (_light != null && !_light.Visible && ease > 0.1f)
					_light.Visible = true;
				break;
			case AnimationType.Rotate:
				RotationDegrees = new Vector3(
					_startRotation.X,
					_startRotation.Y + RotateSpeed * _timer * Mathf.RadToDeg(1f),
					_startRotation.Z
				);
				break;
		}
	}
}

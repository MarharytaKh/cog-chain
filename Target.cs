using Godot;
using System.Collections.Generic;

public partial class Target : Node3D
{
	[Export] public float Radius = 1.23f;
	[Export] public int ToothCount = 20;
	[Export] public int RequiredDirection = 0; // 0 = любое, 1 = положительное, -1 = отрицательное
	
	
	public float angle = 0f;
	public Gear ParentGear = null;
	public List<Gear> Children = new List<Gear>();
	public bool Activated = false;

	private MeshInstance3D meshInstance;
	private StandardMaterial3D material;
	private float emissionCurrent = 0f;
	public Basis _initialBasis;

	public override void _Ready()
	{
		_initialBasis = Transform.Basis;
		meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
		material = meshInstance.GetActiveMaterial(0).Duplicate() as StandardMaterial3D;
		meshInstance.SetSurfaceOverrideMaterial(0, material);
		material.EmissionEnabled = true;
		material.EmissionEnergyMultiplier = 0f;
	}

	public override void _Process(double delta)
	{
		float emissionTarget = ParentGear != null ? 4.0f : 0.6f;
		emissionCurrent = Mathf.Lerp(emissionCurrent, emissionTarget, (float)delta * 1f);
		if (material != null)
			material.EmissionEnergyMultiplier = emissionCurrent;

if (ParentGear == null) return;

float ratio = (float)ParentGear.ToothCount / (float)ToothCount;
angle = (-ParentGear.angle * ratio);
Transform = new Transform3D(
	_initialBasis.Rotated(_initialBasis.Column1.Normalized(), angle),
	Transform.Origin
);

foreach (var c in Children)
	if (IsInstanceValid(c))
		c.UpdateRotation();

// Проверяем направление перед активацией
if (!Activated)
{
	bool dirOk = RequiredDirection == 0
		|| (RequiredDirection > 0 && angle > 0.1f)
		|| (RequiredDirection < 0 && angle < -0.1f);
	if (dirOk) Activate();
}
}

	public bool CanMeshGear(Gear gear)
	{
		float dist = GlobalPosition.DistanceTo(gear.GlobalPosition);
		return Mathf.Abs(dist - (Radius + gear.Radius)) < 0.05f;
	}

	public void Activate()
	{
		if (!Activated)
		{
			Activated = true;

			var animations = GetTree().GetNodesInGroup("LevelAnimation");
			foreach (Node node in animations)
				if (node is LevelAnimation anim)
					anim.Activate();

			var gm = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
			gm?.OnTargetActivated();
		}
	}
}

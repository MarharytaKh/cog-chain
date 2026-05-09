using Godot;
using System.Collections.Generic;

public partial class Target : Node3D
{
	[Export] public float Radius = 1.23f;
	[Export] public int ToothCount = 20;

	public float angle = 0f;
	public Gear ParentGear = null;
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

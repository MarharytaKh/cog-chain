using Godot;
using System.Collections.Generic;

public partial class Motor : Node3D
{
	[Export] public float Speed = 1.0f;
	[Export] public float Radius = 1.23f;
	[Export] public int ToothCount = 20;

	public float angle = 0f;
	public List<Gear> Children = new List<Gear>();

	public override void _Process(double delta)
	{
		angle += (float)delta * Speed;
		Rotation = new Vector3(0, angle, 0);

		foreach (var g in Children)
			g.UpdateRotation();
	}
}

using Godot;
using System.Collections.Generic;

public partial class Motor : Node3D
{
	private Basis _initialBasis;

	public override void _Ready()
	{
		_initialBasis = Transform.Basis;
	}

	[Export] public float Speed = 1.0f;
	[Export] public float Radius = 1.23f;
	[Export] public int ToothCount = 20;

	public float angle = 0f;
	public List<Gear> Children = new List<Gear>();

	public override void _Process(double delta)
	{
	  angle += (float)delta * Speed;
	  angle = angle % (2f * Mathf.Pi);

	  Transform = new Transform3D(
		  _initialBasis.Rotated(_initialBasis.Column1.Normalized(), angle),
		  Transform.Origin
	  );

	  foreach (var g in Children)
	  {
		  if (IsInstanceValid(g))
		  g.UpdateRotation();
	}
}
}

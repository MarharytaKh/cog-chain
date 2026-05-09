using Godot;
using System.Collections.Generic;

public partial class Motor : Node3D
{
	private Basis _initialBasis;
	public Basis initialBasis = Basis.Identity;

	public override void _Ready()
	{
		_initialBasis = Transform.Basis;
		initialBasis = _initialBasis;
	}

	[Export] public float Speed = 1.0f;
	[Export] public float Radius = 1.23f;
	[Export] public int ToothCount = 20;

	public float angle = 0f;
	public List<Gear> Children = new List<Gear>();

	public override void _Process(double delta)
	{
		angle += (float)delta * Speed;

		Transform = new Transform3D(
			_initialBasis.Rotated(_initialBasis.Column1.Normalized(), angle),
			Transform.Origin
		);

		// Только прямые дети — они сами рекурсивно обновят своих детей
		foreach (var g in Children)
		{
			if (IsInstanceValid(g))
				g.UpdateRotation();
		}
	}
}

using Godot;
using System;

public partial class Target : Node3D
{
	public bool Activated = false;

	public void Activate()
	{
		if (!Activated)
		{
			Activated = true;
			GD.Print("LEVEL COMPLETE!");
		}
	}
}

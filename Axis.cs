using Godot;
using System;

public partial class Axis : StaticBody3D
{
	public bool HasGear = false;

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			var spaceState = GetWorld3D().DirectSpaceState;

			var camera = GetViewport().GetCamera3D();
			var mousePos = GetViewport().GetMousePosition();

			var from = camera.ProjectRayOrigin(mousePos);
			var to = from + camera.ProjectRayNormal(mousePos) * 1000;

			var query = PhysicsRayQueryParameters3D.Create(from, to);
			var result = spaceState.IntersectRay(query);

			if (result.Count > 0 && result["collider"].As<GodotObject>() == this)
			{
				GameManager.SelectedAxis = this;
				GD.Print("Axis selected");
			}
		}
	}
}

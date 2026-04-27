using Godot;

public partial class GearClickHandler : StaticBody3D
{
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Right)
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
				// Печатаем всю цепочку родителей
				Node current = this;
				while (current != null)
				{
					GD.Print("Node: " + current.Name + " type=" + current.GetType().Name);
					current = current.GetParent();
				}

				current = this;
				Gear gear = null;
				while (current != null)
				{
					if (current is Gear g) { gear = g; break; }
					current = current.GetParent();
				}

				if (gear != null)
				{
					var gm = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
					gm?.RemoveGear(gear);
				}
				else
					GD.PrintErr("Gear not found in parents!");
			}
		}
	}
}

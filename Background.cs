using Godot;

public partial class Background : StaticBody3D
{
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
		{
			GD.Print("Клик зафиксирован");

			var spaceState = GetWorld3D().DirectSpaceState;
			var camera = GetViewport().GetCamera3D();
			var mousePos = GetViewport().GetMousePosition();

			var from = camera.ProjectRayOrigin(mousePos);
			var to = from + camera.ProjectRayNormal(mousePos) * 1000;

			var query = PhysicsRayQueryParameters3D.Create(from, to);
			var result = spaceState.IntersectRay(query);

			GD.Print("Raycast hits: " + result.Count);

			if (result.Count > 0 && result["collider"].As<GodotObject>() == this)
			{
				GD.Print("Попали в фон!");
				Vector3 hitPos = (Vector3)result["position"];
				var gm = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
				GD.Print("GameManager найден: " + (gm != null));
				gm?.TryPlaceGearAtPosition(hitPos);
			}
		}
	}
}

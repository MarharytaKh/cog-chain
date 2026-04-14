using Godot;
using System;

/// <summary>
/// Oś — punkt umieszczania koła zębatego na poziomie.
/// Obsługuje kliknięcie myszą przez raycast i przekazuje wybór
/// do <see cref="GameManager.SelectedAxis"/>.
/// Dziedziczy po <see cref="StaticBody3D"/> w celu uczestnictwa w fizyce Godot.
/// </summary>
public partial class Axis : StaticBody3D
{
	/// <summary>
	/// <c>true</c>, jeśli na tej osi zostało już umieszczone koło zębate.
	/// Zapobiega ponownemu umieszczeniu koła na zajętej osi.
	/// </summary>
	public bool HasGear = false;

	/// <summary>
	/// Obsługuje naciśnięcie lewego przycisku myszy.
	/// Wykonuje raycast z kamery w kierunku kursora;
	/// jeśli promień przecina tę oś — ustawia ją jako <see cref="GameManager.SelectedAxis"/>.
	/// </summary>
	/// <param name="event">Zdarzenie wejściowe Godot.</param>
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

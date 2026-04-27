using Godot;
using System;

/// <summary>
/// Interaktywny punkt umieszczania koła zębatego na poziomie.
/// Wykrywa kliknięcie myszy przez raycast i ustawia siebie jako
/// <see cref="GameManager.SelectedAxis"/>.
/// </summary>
public partial class Axis : StaticBody3D
{
	/// <summary>
	/// Czy na tej osi zostało już umieszczone koło.
	/// Ustawiane przez <see cref="GameManager._on_button_pressed"/> po udanym umieszczeniu.
	/// Oś z <c>HasGear == true</c> blokuje kolejne umieszczenia.
	/// </summary>
	public bool HasGear = false;

	/// <summary>
	/// Obsługuje kliknięcie lewym przyciskiem myszy — wykonuje raycast z kamery
	/// i jeśli trafiony kolider to ta oś, ustawia <see cref="GameManager.SelectedAxis"/> = this.
	/// </summary>
	/// <param name="event">Zdarzenie wejściowe przekazane przez Godot.</param>
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

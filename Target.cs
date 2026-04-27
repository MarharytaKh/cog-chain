using Godot;
using System.Collections.Generic;

/// <summary>
/// Docelowe koło zębate poziomu — element, który gracz musi wprawić w ruch.
/// Po wykryciu połączenia przez <see cref="PhysicsEngine.BuildGraph"/> wywołuje
/// <see cref="Activate"/>, co kończy poziom.
/// </summary>
public partial class Target : Node3D
{
	/// <summary>
	/// Promień wieńca zębatego w jednostkach sceny.
	/// Używany przez <see cref="PhysicsEngine.BuildGraph"/> do kryterium zazębienia
	/// i przez <see cref="_Process"/> do obliczenia przełożenia.
	/// </summary>
	[Export] public float Radius = 1.23f;

	/// <summary>Liczba zębów — używana do obliczenia przełożenia obrotu.</summary>
	[Export] public int ToothCount = 20;

	/// <summary>Bieżący kąt obrotu celu w radianach. Aktualizowany gdy <see cref="ParentGear"/> != null.</summary>
	public float angle = 0f;

	/// <summary>
	/// Koło bezpośrednio zazębione z celem, dostarczające mu obrotu.
	/// Ustawiane przez <see cref="PhysicsEngine.BuildGraph"/>; zerowane przed każdym przebudowaniem grafu.
	/// </summary>
	public Gear ParentGear = null;

	/// <summary>
	/// Chroni przed wielokrotnym wywołaniem <see cref="GameManager.CompleteLevel"/>
	/// przy kolejnych przebudowaniach grafu po wygranej.
	/// </summary>
	public bool Activated = false;

	private MeshInstance3D meshInstance;
	private StandardMaterial3D material;
	private float emissionCurrent = 0f;

	/// <summary>
	/// Pobiera siatkę, duplikuje materiał i włącza emisję.
	/// Duplikacja materiału zapobiega wpływaniu zmian emisji na inne obiekty w scenie.
	/// </summary>
	public override void _Ready()
	{
		meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
		material = meshInstance.GetActiveMaterial(0).Duplicate() as StandardMaterial3D;
		meshInstance.SetSurfaceOverrideMaterial(0, material);
		material.EmissionEnabled = true;
		material.EmissionEnergyMultiplier = 0f;
	}

	/// <summary>
	/// Interpoluje emisję materiału (4.0 gdy podłączony, 0.6 gdy odłączony)
	/// i obraca cel zgodnie z przełożeniem koła nadrzędnego.
	/// </summary>
	/// <param name="delta">Czas od poprzedniej klatki w sekundach.</param>
public override void _Process(double delta)
{
	float emissionTarget = ParentGear != null ? 4.0f : 0.6f;
	emissionCurrent = Mathf.Lerp(emissionCurrent, emissionTarget, (float)delta * 1f);
	if (material != null)
		material.EmissionEnergyMultiplier = emissionCurrent;

	if (ParentGear == null) return;

	// Используем ToothCount как шестерёнки, а не Radius
	float ratio = (float)ParentGear.ToothCount / (float)ToothCount;
	angle = (-ParentGear.angle * ratio);
	Rotation = new Vector3(0, angle, 0);
}

	/// <summary>
	/// Sprawdza, czy podane koło może zazębić się z celem (tolerancja 0.05 jednostki sceny).
	/// </summary>
	/// <param name="gear">Koło do sprawdzenia.</param>
	/// <returns><c>true</c> jeśli koło zazębia się z celem.</returns>
	public bool CanMeshGear(Gear gear)
	{
		float dist = GlobalPosition.DistanceTo(gear.GlobalPosition);
		return Mathf.Abs(dist - (Radius + gear.Radius)) < 0.05f;
	}

	/// <summary>
	/// Oznacza cel jako wygrany i wywołuje <see cref="GameManager.CompleteLevel"/>.
	/// Wykonuje się jednokrotnie dzięki fladze <see cref="Activated"/>.
	/// </summary>
	public void Activate()
	{
		if (!Activated)
		{
			Activated = true;
			var gm = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
			gm.CompleteLevel();
		}
	}
}

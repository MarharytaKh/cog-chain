using Godot;
using System.Collections.Generic;

/// <summary>
/// Docelowe koło zębate poziomu — końcowy element mechanizmu, który należy wprawić w ruch.
/// Śledzi połączenie przez <see cref="ParentGear"/> i wizualnie reaguje na nie
/// płynną zmianą emisji świetlnej. Po połączeniu obraca się synchronicznie
/// z łańcuchem przekładni i aktywuje zakończenie poziomu.
/// </summary>
public partial class Target : Node3D
{
	/// <summary>
	/// Promień wieńca zębatego celu w jednostkach sceny.
	/// </summary>
	[Export] public float Radius = 1.23f;

	/// <summary>
	/// Liczba zębów celu. Używana do obliczenia przełożenia.
	/// </summary>
	[Export] public int ToothCount = 20;

	/// <summary>
	/// Bieżący kąt obrotu celu w radianach.
	/// </summary>
	public float angle = 0f;

	/// <summary>
	/// Koło zębate bezpośrednio zazębione z celem.
	/// <c>null</c>, jeśli łańcuch nie jest podłączony.
	/// Ustawiane przez <see cref="PhysicsEngine.BuildGraph"/>.
	/// </summary>
	public Gear ParentGear = null;

	/// <summary>
	/// Flaga aktywacji: <c>true</c>, jeśli cel został już aktywowany.
	/// Zapobiega wielokrotnemu wywołaniu zakończenia poziomu.
	/// </summary>
	public bool Activated = false;

	private MeshInstance3D meshInstance;
	private StandardMaterial3D material;
	private float emissionCurrent = 0f;

	/// <summary>
	/// Pobiera referencję do siatki, duplikuje materiał i włącza emisję.
	/// Wywoływana przez Godot przy dodaniu węzła do drzewa sceny.
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
	/// Aktualizuje emisję świetlną i kąt obrotu celu każdą klatkę.
	/// Emisja jest płynnie interpolowana do 4.0 przy podłączonym łańcuchu i 0.6 bez niego.
	/// Obrót obliczany na podstawie przełożenia z <see cref="ParentGear"/>.
	/// </summary>
	/// <param name="delta">Czas od poprzedniej klatki (sekundy).</param>
	public override void _Process(double delta)
	{
		float emissionTarget = ParentGear != null ? 4.0f : 0.6f;
		emissionCurrent = Mathf.Lerp(emissionCurrent, emissionTarget, (float)delta * 1f);
		if (material != null)
			material.EmissionEnergyMultiplier = emissionCurrent;

		if (ParentGear == null) return;

		float ratio = ParentGear.Radius / Radius;
		angle = -ParentGear.angle * ratio;
		Rotation = new Vector3(0, angle, 0);
	}

	/// <summary>
	/// Sprawdza, czy dane koło zębate może zazębić się z celem na podstawie odległości.
	/// Tolerancja zazębienia wynosi 0.05 jednostki.
	/// </summary>
	/// <param name="gear">Koło zębate do sprawdzenia.</param>
	/// <returns><c>true</c>, jeśli zazębienie jest możliwe.</returns>
	public bool CanMeshGear(Gear gear)
	{
		float dist = GlobalPosition.DistanceTo(gear.GlobalPosition);
		return Mathf.Abs(dist - (Radius + gear.Radius)) < 0.05f;
	}

	/// <summary>
	/// Oznacza cel jako aktywowany i wywołuje zakończenie poziomu w <see cref="GameManager"/>.
	/// Wywoływana jednokrotnie z <see cref="PhysicsEngine.BuildGraph"/> po wykryciu zazębienia.
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

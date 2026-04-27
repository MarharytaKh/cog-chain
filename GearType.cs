using Godot;

/// <summary>
/// Zasób konfiguracyjny opisujący jeden typ koła zębatego.
/// Zapisywany jako plik <c>.tres</c>; może być współdzielony przez wiele poziomów.
/// </summary>
[GlobalClass]
public partial class GearType : Resource
{
	/// <summary>Wyświetlana nazwa typu koła (np. „Małe", „Duże"). Widoczna na przyciskach UI.</summary>
	[Export] public string gearName = "Standard";

	/// <summary>
	/// Promień wieńca zębatego w jednostkach sceny.
	/// Musi odpowiadać promieniowi siatki 3D prefabu — używany przez
	/// <see cref="PhysicsEngine.BuildGraph"/> do wykrywania zazębienia.
	/// </summary>
	[Export] public float Radius = 1.23f;

	/// <summary>
	/// Liczba zębów koła. Wyznacza przełożenie:
	/// <c>ratio = drivingToothCount / thisToothCount</c>.
	/// </summary>
	[Export] public int ToothCount = 20;

	/// <summary>Mnożnik prędkości — zarezerwowany na przyszłe rozszerzenia; aktualnie nieużywany.</summary>
	[Export] public float SpeedMultiplier = 1.0f;

	/// <summary>
	/// Korekcja kątowa (w radianach) dla wyrównania wizualnego zębów modelu 3D.
	/// Dodawana do <c>phaseOffset</c> w <see cref="Gear.SnapPhaseWithMotor"/>.
	/// </summary>
	[Export] public float AngleOffset = 0f;

	/// <summary>
	/// Scena-prefab instancjonowana przy umieszczaniu koła na osi.
	/// Węzeł główny musi być typu <see cref="Gear"/>.
	/// </summary>
	[Export] public PackedScene scenePrefab;
}

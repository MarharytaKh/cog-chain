using Godot;

/// <summary>
/// Zasób konfiguracji typu koła zębatego.
/// Definiuje wszystkie statyczne parametry koła: nazwę, właściwości fizyczne
/// oraz referencję do sceny do instancjonowania.
/// Tworzony jako zasób <c>.tres</c> i przypisywany przez inspektor Godot.
/// </summary>
[GlobalClass]
public partial class GearType : Resource
{
	/// <summary>
	/// Wyświetlana nazwa typu koła zębatego (np. „Małe", „Duże").
	/// Używana w przyciskach interfejsu użytkownika.
	/// </summary>
	[Export] public string gearName = "Standard";

	/// <summary>
	/// Promień wieńca zębatego w jednostkach sceny.
	/// </summary>
	[Export] public float Radius = 1.23f;

	/// <summary>
	/// Liczba zębów koła zębatego.
	/// Określa przełożenie przy zazębieniu.
	/// </summary>
	[Export] public int ToothCount = 20;

	/// <summary>
	/// Mnożnik prędkości dla tego typu koła zębatego.
	/// Zarezerwowane na potrzeby rozbudowy mechaniki przełożeń.
	/// </summary>
	[Export] public float SpeedMultiplier = 1.0f;

	/// <summary>
	/// Dodatkowe przesunięcie kątowe (rad) stosowane przy przyciąganiu fazy.
	/// Pozwala wizualnie wyrównać zęby niestandardowych modeli.
	/// </summary>
	[Export] public float AngleOffset = 0f;

	/// <summary>
	/// Scena-prefab koła zębatego, instancjonowana przy umieszczaniu na osi.
	/// Węzeł główny musi być typu <see cref="Gear"/>.
	/// </summary>
	[Export] public PackedScene scenePrefab;
}

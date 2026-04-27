using Godot;

/// <summary>
/// Zasób konfiguracyjny opisujący jeden poziom gry.
/// Zapisywany jako plik <c>.tres</c>; przechowywany w tablicy <see cref="GameManager.levels"/>.
/// </summary>
/// <remarks>
/// Długości tablic <see cref="availableGearTypes"/> i <see cref="availableGearCounts"/>
/// muszą być identyczne — niezgodność powoduje wyjątek w <see cref="GameManager.SetupGearButtons"/>.
/// </remarks>
[GlobalClass]
public partial class Level : Resource
{
	/// <summary>Unikalny identyfikator poziomu — używany przy debugowaniu i zapisie postępu.</summary>
	[Export] public int LevelID = 1;

	/// <summary>Maksymalna liczba kół zębatych dostępna dla gracza na poziomie.</summary>
	[Export] public int MaxGears = 5;

	/// <summary>Wskaźnik trudności poziomu — nie wpływa bezpośrednio na mechanikę.</summary>
	[Export] public int Difficulty = 1;

	/// <summary>Minimalna liczba gwiazdek wymagana do odblokowania poziomu.</summary>
	[Export] public int starsRequired = 1;

	/// <summary>Czy poziom jest dostępny dla gracza. Ustawiany przez <see cref="Unlock"/>.</summary>
	[Export] public bool isUnlocked = true;

	/// <summary>
	/// Scena ładowana przy przejściu do tego poziomu.
	/// Przekazywana do <c>GetTree().ChangeSceneToPacked()</c> przez <see cref="GameManager.LoadNextLevel"/>.
	/// </summary>
	[Export] public PackedScene levelScene;

	/// <summary>
	/// Typy kół dostępnych dla gracza. Indeks <c>i</c> jest zsynchronizowany
	/// z <see cref="availableGearCounts"/>[i].
	/// </summary>
	[Export] public GearType[] availableGearTypes;

	/// <summary>
	/// Limity ilościowe kół każdego typu. Musi mieć identyczną długość
	/// co <see cref="availableGearTypes"/>.
	/// </summary>
	[Export] public int[] availableGearCounts;

	/// <summary>Odblokowuje poziom. Wywoływana przez zewnętrzny system postępu.</summary>
	public void Unlock()
	{
		isUnlocked = true;
	}
}

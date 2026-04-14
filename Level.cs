using Godot;

/// <summary>
/// Zasób konfiguracji poziomu gry.
/// Przechowuje ustawienia trudności, dostępne typy kół zębatych,
/// referencję do sceny oraz wymagania dotyczące odblokowania.
/// </summary>
[GlobalClass]
public partial class Level : Resource
{
	/// <summary>
	/// Unikalny identyfikator poziomu.
	/// </summary>
	[Export] public int LevelID = 1;

	/// <summary>
	/// Maksymalna liczba kół zębatych, które można umieścić na poziomie.
	/// </summary>
	[Export] public int MaxGears = 5;

	/// <summary>
	/// Numeryczny wskaźnik trudności poziomu.
	/// </summary>
	[Export] public int Difficulty = 1;

	/// <summary>
	/// Minimalna liczba gwiazdek wymagana do odblokowania tego poziomu.
	/// </summary>
	[Export] public int starsRequired = 1;

	/// <summary>
	/// Określa, czy poziom jest odblokowany dla gracza.
	/// Domyślnie <c>true</c>.
	/// </summary>
	[Export] public bool isUnlocked = true;

	/// <summary>
	/// Scena ładowana przy przejściu do tego poziomu.
	/// </summary>
	[Export] public PackedScene levelScene;

	/// <summary>
	/// Tablica dostępnych typów kół zębatych na tym poziomie.
	/// </summary>
	[Export] public GearType[] availableGearTypes;

	/// <summary>
	/// Liczba dostępnych kół każdego typu.
	/// Indeksy odpowiadają indeksom w <see cref="availableGearTypes"/>.
	/// </summary>
	[Export] public int[] availableGearCounts;

	/// <summary>
	/// Odblokowuje poziom, ustawiając <see cref="isUnlocked"/> na <c>true</c>.
	/// </summary>
	public void Unlock()
	{
		isUnlocked = true;
	}
}

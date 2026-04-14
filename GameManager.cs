using Godot;
using System.Collections.Generic;

/// <summary>
/// Centralny menedżer gry. Zarządza ładowaniem poziomów, umieszczaniem kół zębatych,
/// przyciskami interfejsu użytkownika do wyboru typu koła oraz przeliczaniem grafu przekładni.
/// Istnieje jako węzeł-singleton w drzewie sceny (grupa <c>"GameManager"</c>).
/// </summary>
public partial class GameManager : Node
{
	/// <summary>
	/// Oś wybrana przez gracza kliknięciem myszy.
	/// <c>null</c>, jeśli żadna oś nie jest wybrana.
	/// Ustawiana przez <see cref="Axis._Input"/>.
	/// </summary>
	public static Axis SelectedAxis;

	/// <summary>
	/// Typ koła zębatego wybrany do umieszczenia.
	/// Ustawiany po naciśnięciu przycisku interfejsu.
	/// </summary>
	public static GearType SelectedGearConfig;

	/// <summary>
	/// Tablica wszystkich poziomów gry w kolejności przechodzenia.
	/// Przypisywana przez inspektor Godot.
	/// </summary>
	[Export] public Level[] levels;

	private Level currentLevel;
	private int currentLevelIndex = 0;

	/// <summary>
	/// Liczniki pozostałych kół zębatych dla każdego typu na bieżącym poziomie.
	/// Indeksy odpowiadają <see cref="Level.availableGearTypes"/>.
	/// </summary>
	private int[] remainingGearCounts;

	private Motor motor;
	private Target target;
	private Node uiInstance;

	/// <summary>
	/// Inicjalizuje menedżera: ładuje bieżący poziom, wyszukuje silnik i cel,
	/// tworzy instancję interfejsu użytkownika i wywołuje <see cref="SetupLevel"/>.
	/// </summary>
	public override void _Ready()
	{
		AddToGroup("GameManager");

		if (levels == null || levels.Length == 0)
		{
			GD.PrintErr("Levels array is empty!");
			return;
		}

		currentLevel = levels[currentLevelIndex];
		motor = GetTree().CurrentScene.GetNode<Motor>("Motor");
		target = GetTree().CurrentScene.GetNode<Target>("Target");

		var uiScene = GD.Load<PackedScene>("res://UI/UILayer.tscn");
		uiInstance = uiScene.Instantiate();
		GetTree().Root.CallDeferred("add_child", uiInstance);

		Callable.From(SetupLevel).CallDeferred();
	}

	/// <summary>
	/// Inicjalizuje liczniki kół z danych bieżącego poziomu,
	/// ustawia domyślny typ i tworzy przyciski interfejsu.
	/// </summary>
	private void SetupLevel()
	{
		GD.Print($"SetupLevel called, types={currentLevel.availableGearTypes?.Length ?? 0}, counts={currentLevel.availableGearCounts?.Length ?? 0}");

		if (currentLevel.availableGearCounts != null)
		{
			remainingGearCounts = new int[currentLevel.availableGearCounts.Length];
			for (int i = 0; i < currentLevel.availableGearCounts.Length; i++)
				remainingGearCounts[i] = currentLevel.availableGearCounts[i];
		}

		if (currentLevel.availableGearTypes != null && currentLevel.availableGearTypes.Length > 0)
			SelectedGearConfig = currentLevel.availableGearTypes[0];

		SetupGearButtons();
	}

	/// <summary>
	/// Przypisuje procedury obsługi naciśnięcia do przycisków interfejsu wyboru kół.
	/// Każdy przycisk wyświetla nazwę typu i pozostały licznik.
	/// Naciśnięcie przy zerowym liczniku jest ignorowane.
	/// </summary>
	private void SetupGearButtons()
	{
		GD.Print($"SetupGearButtons, uiInstance={uiInstance?.Name}");

		if (currentLevel.availableGearTypes == null)
		{
			GD.PrintErr("availableGearTypes is null!");
			return;
		}

		for (int i = 0; i < currentLevel.availableGearTypes.Length; i++)
		{
			int index = i;
			var btn = uiInstance.GetNodeOrNull<Button>($"UI/Panel/Button{i}");
			if (btn != null)
			{
				GD.Print($"Button{i} found!");
				UpdateButtonText(btn, index);
				btn.Pressed += () =>
				{
					if (remainingGearCounts[index] <= 0)
					{
						GD.Print("No gears left!");
						return;
					}
					SelectedGearConfig = currentLevel.availableGearTypes[index];
					_on_button_pressed(index);
				};
			}
			else
				GD.PrintErr($"Button{i} not found at path UI/Panel/Button{i}");
		}
	}

	/// <summary>
	/// Aktualizuje tekst przycisku: nazwę typu koła i pozostałą liczbę.
	/// </summary>
	/// <param name="btn">Przycisk do aktualizacji.</param>
	/// <param name="index">Indeks typu koła w tablicy poziomu.</param>
	private void UpdateButtonText(Button btn, int index)
	{
		var type = currentLevel.availableGearTypes[index];
		btn.Text = $"{type.gearName}\n{remainingGearCounts[index]}";
	}

	/// <summary>
	/// Obsługuje naciśnięcie przycisku interfejsu: sprawdza wybraną oś, dostępność konfiguracji,
	/// kolizje z innymi kołami i silnikiem, następnie tworzy instancję koła,
	/// synchronizuje fazę i wywołuje przeliczenie grafu.
	/// </summary>
	/// <param name="gearIndex">Indeks wybranego typu koła zębatego.</param>
	private void _on_button_pressed(int gearIndex)
	{
		GD.Print($"Button pressed, axis={SelectedAxis}, config={SelectedGearConfig?.gearName}");

		if (SelectedAxis == null || SelectedAxis.HasGear) return;

		if (SelectedGearConfig == null)
		{
			GD.PrintErr("SelectedGearConfig is null!");
			return;
		}

		if (SelectedGearConfig.scenePrefab == null)
		{
			GD.PrintErr($"scenePrefab not assigned in {SelectedGearConfig.gearName}!");
			return;
		}

		Vector3 targetPos = SelectedAxis.GlobalPosition;
		float newRadius = SelectedGearConfig.Radius;

		foreach (var g in GetAllGears())
		{
			float dist = targetPos.DistanceTo(g.GlobalPosition);
			if (dist < (newRadius + g.Radius) - 0.1f)
			{
				GD.Print("Overlap with gear!");
				return;
			}
		}

		float distMotor = targetPos.DistanceTo(motor.GlobalPosition);
		if (distMotor < (newRadius + motor.Radius) - 0.1f)
		{
			GD.Print("Overlap with motor!");
			return;
		}

		var gear = SelectedGearConfig.scenePrefab.Instantiate<Gear>();
		gear.config = SelectedGearConfig;
		gear.Radius = SelectedGearConfig.Radius;
		gear.ToothCount = SelectedGearConfig.ToothCount;

		AddChild(gear);
		gear.GlobalPosition = targetPos;

		float distToMotor = targetPos.DistanceTo(motor.GlobalPosition);
		if (Mathf.Abs(distToMotor - (newRadius + motor.Radius)) < 0.5f)
			gear.SnapPhaseWithMotor(motor);
		else
		{
			foreach (var g in GetAllGears())
			{
				if (g == gear) continue;
				float dist = targetPos.DistanceTo(g.GlobalPosition);
				if (Mathf.Abs(dist - (newRadius + g.Radius)) < 0.5f)
				{
					gear.SnapPhaseWithGear(g);
					break;
				}
			}
		}

		SelectedAxis.HasGear = true;
		Recalculate();
	}

	/// <summary>
	/// Przebudowuje graf przekładni przez <see cref="PhysicsEngine.BuildGraph"/>
	/// na podstawie bieżącej listy kół zębatych.
	/// </summary>
	private void Recalculate()
	{
		var gears = GetAllGears();
		if (target == null)
		{
			GD.PrintErr("Target not found!");
			return;
		}
		PhysicsEngine.BuildGraph(motor, gears, target);
	}

	/// <summary>
	/// Kończy bieżący poziom: wypisuje komunikat i po 2 sekundach ładuje następny.
	/// </summary>
	public void CompleteLevel()
	{
		GD.Print("Level complete!");
		GetTree().CreateTimer(2.0).Timeout += () => LoadNextLevel();
	}

	/// <summary>
	/// Ładuje następny poziom według indeksu.
	/// Jeśli poziomy się skończyły — wypisuje komunikat o końcu gry.
	/// </summary>
	public void LoadNextLevel()
	{
		currentLevelIndex++;
		if (currentLevelIndex < levels.Length)
		{
			var next = levels[currentLevelIndex];
			if (next.levelScene != null)
				GetTree().ChangeSceneToPacked(next.levelScene);
		}
		else
			GD.Print("Last level reached!");
	}

	/// <summary>
	/// Zwraca listę wszystkich kół zębatych będących węzłami podrzędnymi GameManager.
	/// </summary>
	/// <returns>Lista aktywnych kół zębatych na poziomie.</returns>
	private List<Gear> GetAllGears()
	{
		var gears = new List<Gear>();
		foreach (Node node in GetChildren())
			if (node is Gear g)
				gears.Add(g);
		return gears;
	}
}

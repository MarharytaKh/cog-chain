using Godot;
using System.Collections.Generic;

/// <summary>
/// Centralny menedżer gry — koordynuje inicjalizację poziomów, UI, umieszczanie kół
/// i przejścia między poziomami. Rejestruje się w grupie Godot <c>"GameManager"</c>,
/// dzięki czemu inne węzły mogą go znaleźć przez <c>GetFirstNodeInGroup</c>.
/// </summary>
public partial class GameManager : Node
{
	/// <summary>
	/// Oś aktualnie wybrana przez gracza. Ustawiana przez <see cref="Axis._Input"/>,
	/// odczytywana przez <see cref="_on_button_pressed"/>.
	/// </summary>
	public static Axis SelectedAxis;

	/// <summary>
	/// Typ koła wybrany do umieszczenia. Ustawiany przez <see cref="SetupGearButtons"/>
	/// i przez obsługę przycisków UI.
	/// </summary>
	public static GearType SelectedGearConfig;

	/// <summary>Tablica wszystkich poziomów gry w kolejności przechodzenia. Przypisywana przez inspektor.</summary>
	[Export] public Level[] levels;

	private Level currentLevel;
	private int currentLevelIndex = 0;

	/// <summary>
	/// Robocze liczniki pozostałych kół dla każdego typu.
	/// Kopia <see cref="Level.availableGearCounts"/> — chroniona przed modyfikacją zasobu.
	/// </summary>
	private int[] remainingGearCounts;

	private Motor motor;
	private Target target;
	private Node uiInstance;

	/// <summary>
	/// Rejestruje menedżera w grupie, wyszukuje węzły <c>Motor</c> i <c>Target</c>,
	/// ładuje UI i odkłada inicjalizację poziomu przez <c>CallDeferred</c>
	/// (aby UI było już w drzewie sceny).
	/// </summary>
public override void _Ready()
{
	AddToGroup("GameManager");
	GetTree().SceneChanged += OnSceneChanged;
	CallDeferred(nameof(InitLevel)); // вернули
}

private void OnSceneChanged()
{
	CallDeferred(nameof(InitLevel));
}

private void InitLevel()
{    // Удаляем все шестерёнки от прошлого уровня
	foreach (Node node in GetChildren())
		if (node is Gear)
			node.QueueFree();

	var scene = GetTree().CurrentScene;
	if (scene == null) return;

	motor = scene.GetNodeOrNull<Motor>("Motor");
	target = scene.GetNodeOrNull<Target>("Target");

	// не уровень (например главное меню) — пропускаем
	if (motor == null || target == null) return;

	if (levels == null || levels.Length == 0)
	{
		GD.PrintErr("levels пустой!");
		return;
	}

	currentLevel = levels[currentLevelIndex];

	var uiManager = GetNodeOrNull<UIManager>("/root/UIManager");
	if (uiManager == null) { GD.PrintErr("UIManager не найден!"); return; }

	uiManager.Show("hud");
	uiInstance = uiManager.GetCurrentScreen();

	SetupLevel();
}

public void CompleteLevel()
{
	GD.Print($"LEVEL COMPLETE! levels={levels?.Length.ToString() ?? "NULL"}");
	var uiManager = GetNodeOrNull<UIManager>("/root/UIManager");
	if (uiManager == null) { GD.PrintErr("UIManager null!"); return; }
	if (levels == null) { GD.PrintErr("levels null!"); return; }
	uiManager.Show("level_complete");
	var screen = uiManager.GetCurrentScreen();
	if (screen is LevelCompleteScreen lcs)
		lcs.Setup(currentLevelIndex, levels.Length);
	else
		GD.PrintErr($"screen type={screen?.GetType().Name}");
}
public void RestartLevel()
{
	var scene = levels[currentLevelIndex].levelScene;
	if (scene != null)
		GetTree().ChangeSceneToPacked(scene);
}

	/// <summary>
	/// Kopiuje liczniki kół z konfiguracji poziomu, ustawia domyślny typ koła
	/// i inicjuje przyciski UI przez <see cref="SetupGearButtons"/>.
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
	/// Wiąże przyciski UI (<c>Button0</c>, <c>Button1</c>, …) z typami kół dostępnymi
	/// na poziomie. Każdy przycisk ustawia <see cref="SelectedGearConfig"/> i wywołuje
	/// <see cref="_on_button_pressed"/>.
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
		 var removeBtn = uiInstance.GetNodeOrNull<Button>("UI/Panel/RemoveButton");
		 if (removeBtn != null)
		  removeBtn.Pressed += () => RemoveGearFromAxis(SelectedAxis);
		 }
public void RemoveGearFromAxis(Axis axis)
{
	if (axis == null || !axis.HasGear) return;

	foreach (Node node in GetChildren())
	{
		if (node is Gear gear && gear.GlobalPosition.DistanceTo(axis.GlobalPosition) < 0.1f)
		{
			// Останавливаем мотор чтобы он не вызывал UpdateRotation
			motor.SetProcess(false);

			// Сбрасываем весь граф
			motor.Children.Clear();
			foreach (Node n in GetChildren())
				if (n is Gear g)
					g.Reset();

			for (int i = 0; i < currentLevel.availableGearTypes.Length; i++)
			{
				if (currentLevel.availableGearTypes[i] == gear.config)
				{
					remainingGearCounts[i]++;
					var btn = uiInstance.GetNodeOrNull<Button>($"UI/Panel/Button{i}");
					if (btn != null) UpdateButtonText(btn, i);
					break;
				}
			}

			axis.HasGear = false;
			gear.QueueFree();

			// На следующем кадре пересчитываем и включаем мотор
			CallDeferred(nameof(ReenableMotorAndRecalculate));
			return;
		}
	}
}

private void ReenableMotorAndRecalculate()
{
	motor.SetProcess(true);
	Recalculate();
}

	/// <summary>
	/// Aktualizuje tekst przycisku: <c>"{gearName}\n{remainingCount}"</c>.
	/// </summary>
	/// <param name="btn">Przycisk do zaktualizowania.</param>
	/// <param name="index">Indeks typu koła w tablicach poziomu.</param>
	private void UpdateButtonText(Button btn, int index)
	{
		var type = currentLevel.availableGearTypes[index];
		btn.Text = $"{type.gearName}\n{remainingGearCounts[index]}";
	}

	/// <summary>
	/// Obsługuje naciśnięcie przycisku wyboru koła: waliduje oś i konfigurację,
	/// sprawdza nakładanie z istniejącymi kołami i silnikiem, tworzy instancję koła,
	/// synchronizuje fazę zazębienia i przebudowuje graf przez <see cref="Recalculate"/>.
	/// </summary>
	/// <param name="gearIndex">Indeks wybranego typu koła w tablicy <see cref="Level.availableGearTypes"/>.</param>
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
		float distTarget = targetPos.DistanceTo(target.GlobalPosition);
	   if (distTarget < (newRadius + target.Radius) - 0.1f)
	   {
			GD.Print("Overlap with target!");
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
		for (int i = 0; i < currentLevel.availableGearTypes.Length; i++)
		{
		   if (currentLevel.availableGearTypes[i] == SelectedGearConfig)
		{
		   remainingGearCounts[i]--;
		   var btn = uiInstance.GetNodeOrNull<Button>($"UI/Panel/Button{i}");
		   if (btn != null) UpdateButtonText(btn, i);
		  break;
		}
}
		Recalculate();
	}

	/// <summary>
	/// Pobiera listę aktywnych kół i zleca przebudowanie grafu przez <see cref="PhysicsEngine.BuildGraph"/>.
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
	/// Ładuje następny poziom z tablicy <see cref="levels"/> lub loguje komunikat
	/// o ukończeniu całej gry, gdy nie ma już więcej poziomów.
	/// </summary>
	public void LoadNextLevel()
	{
		currentLevelIndex++;
		GD.Print($"Загружаем уровень {currentLevelIndex}, сцена={levels[currentLevelIndex].levelScene?.ResourcePath}");
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
	/// Zwraca listę wszystkich węzłów <see cref="Gear"/> będących dziećmi tego menedżera.
	/// </summary>
	/// <returns>Nowa lista aktywnych kół zębatych.</returns>
	private List<Gear> GetAllGears()
	{
		var gears = new List<Gear>();
		foreach (Node node in GetChildren())
			if (node is Gear g)
				gears.Add(g);
		return gears;
	}
}

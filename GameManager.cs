using Godot;
using System.Collections.Generic;

public partial class GameManager : Node
{
	public static GearType SelectedGearConfig;

	[Export] public Level[] levels;
	[Export] public PackedScene axisPrefab;

	private Level currentLevel;
	private int currentLevelIndex = 0;
	private int[] remainingGearCounts;

	private Motor motor;
	private Target target;
	private Node uiInstance;

	// Список спауненных осей — чтобы удалять их при смене уровня
	private List<Node3D> spawnedAxes = new List<Node3D>();

	public override void _Ready()
	{
		AddToGroup("GameManager");
		GetTree().SceneChanged += OnSceneChanged;
		CallDeferred(nameof(InitLevel));
	}

	private void OnSceneChanged()
	{
		CallDeferred(nameof(InitLevel));
	}

	private void InitLevel()
	{
		// Удаляем шестерёнки от прошлого уровня
		foreach (Node node in GetChildren())
			if (node is Gear)
				node.QueueFree();

		// Удаляем оси от прошлого уровня
		foreach (var axis in spawnedAxes)
			if (IsInstanceValid(axis))
				axis.QueueFree();
		spawnedAxes.Clear();

		var scene = GetTree().CurrentScene;
		if (scene == null) return;

		motor = scene.GetNodeOrNull<Motor>("Motor");
		target = scene.GetNodeOrNull<Target>("Target");

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

	private void SetupLevel()
	{
		GD.Print($"SetupLevel called, types={currentLevel.availableGearTypes?.Length ?? 0}");

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
				};
			}
			else
				GD.PrintErr($"Button{i} not found at path UI/Panel/Button{i}");
		}
	}

	private void UpdateButtonText(Button btn, int index)
	{
		var type = currentLevel.availableGearTypes[index];
		btn.Text = $"{type.gearName}\n{remainingGearCounts[index]}";
	}

	/// <summary>
	/// Вызывается из Background.cs при клике по полю.
	/// Ставит шестерёнку + ось в указанную позицию.
	/// </summary>
	public void TryPlaceGearAtPosition(Vector3 worldPos)
	{
		if (SelectedGearConfig == null) return;
		if (SelectedGearConfig.scenePrefab == null)
		{
			GD.PrintErr($"scenePrefab not assigned in {SelectedGearConfig.gearName}!");
			return;
		}

		float newRadius = SelectedGearConfig.Radius;

		// Снэппим Y к высоте мотора чтобы всё было на одной плоскости
		worldPos.Y = motor.GlobalPosition.Y;

		// Проверка наложения с существующими шестерёнками
		foreach (var g in GetAllGears())
		{
			float dist = worldPos.DistanceTo(g.GlobalPosition);
			if (dist < (newRadius + g.Radius) - 0.1f)
			{
				GD.Print("Overlap with gear!");
				return;
			}
		}

		// Проверка наложения с мотором
		float distMotor = worldPos.DistanceTo(motor.GlobalPosition);
		if (distMotor < (newRadius + motor.Radius) - 0.1f)
		{
			GD.Print("Overlap with motor!");
			return;
		}

		// Проверка наложения с таргетом
		float distTarget = worldPos.DistanceTo(target.GlobalPosition);
		if (distTarget < (newRadius + target.Radius) - 0.1f)
		{
			GD.Print("Overlap with target!");
			return;
		}

		// Проверяем счётчик
		int index = -1;
		for (int i = 0; i < currentLevel.availableGearTypes.Length; i++)
		{
			if (currentLevel.availableGearTypes[i] == SelectedGearConfig)
			{
				index = i;
				break;
			}
		}
		if (index == -1 || remainingGearCounts[index] <= 0)
		{
			GD.Print("No gears left!");
			return;
		}

		// Спауним ось если есть префаб
		if (axisPrefab != null)
		{
			var axisNode = axisPrefab.Instantiate<Node3D>();
			GetTree().CurrentScene.AddChild(axisNode);
			axisNode.GlobalPosition = worldPos;
			spawnedAxes.Add(axisNode);
		}

		// Спауним шестерёнку
		var gear = SelectedGearConfig.scenePrefab.Instantiate<Gear>();
		gear.config = SelectedGearConfig;
		gear.Radius = SelectedGearConfig.Radius;
		gear.ToothCount = SelectedGearConfig.ToothCount;
		AddChild(gear);
		gear.GlobalPosition = worldPos;

		// Синхронизируем фазу
		if (Mathf.Abs(distMotor - (newRadius + motor.Radius)) < 0.5f)
			gear.SnapPhaseWithMotor(motor);
		else
		{
			foreach (var g in GetAllGears())
			{
				if (g == gear) continue;
				float dist = worldPos.DistanceTo(g.GlobalPosition);
				if (Mathf.Abs(dist - (newRadius + g.Radius)) < 0.5f)
				{
					gear.SnapPhaseWithGear(g);
					break;
				}
			}
		}

		remainingGearCounts[index]--;
		var btn = uiInstance.GetNodeOrNull<Button>($"UI/Panel/Button{index}");
		if (btn != null) UpdateButtonText(btn, index);

		Recalculate();
	}

	/// <summary>
	/// Вызывается из GearClickHandler.cs при правом клике на шестерёнку.
	/// </summary>
	public void RemoveGear(Gear gear)
	{
		if (gear == null || !IsInstanceValid(gear)) return;

		motor.SetProcess(false);
		motor.Children.Clear();
		foreach (Node n in GetChildren())
			if (n is Gear g && IsInstanceValid(g))
				g.Reset();

		// Удаляем ось рядом с шестерёнкой
		for (int i = spawnedAxes.Count - 1; i >= 0; i--)
		{
			var axis = spawnedAxes[i];
			if (!IsInstanceValid(axis)) { spawnedAxes.RemoveAt(i); continue; }
			if (axis.GlobalPosition.DistanceTo(gear.GlobalPosition) < 0.1f)
			{
				axis.QueueFree();
				spawnedAxes.RemoveAt(i);
				break;
			}
		}

		// Возвращаем счётчик
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

		gear.QueueFree();
		CallDeferred(nameof(ReenableMotorAndRecalculate));
	}

	private void ReenableMotorAndRecalculate()
	{
		motor.SetProcess(true);
		Recalculate();
	}

	private void Recalculate()
	{
		var gears = GetAllGears();
		if (target == null) { GD.PrintErr("Target not found!"); return; }
		PhysicsEngine.BuildGraph(motor, gears, target);
	}

	public void LoadNextLevel()
	{
		currentLevelIndex++;
		GD.Print($"Загружаем уровень {currentLevelIndex}");
		if (currentLevelIndex < levels.Length)
		{
			var next = levels[currentLevelIndex];
			if (next.levelScene != null)
				GetTree().ChangeSceneToPacked(next.levelScene);
		}
		else
			GD.Print("Last level reached!");
	}

	private List<Gear> GetAllGears()
	{
		var gears = new List<Gear>();
		foreach (Node node in GetChildren())
			if (node is Gear g && IsInstanceValid(g))
				gears.Add(g);
		return gears;
	}
}

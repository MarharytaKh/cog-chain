using Godot;
using System.Collections.Generic;

public partial class GameManager : Node
{
	public static Axis SelectedAxis;
	public static GearType SelectedGearConfig;

	[Export] public Level[] levels;

	private Level currentLevel;
	private int currentLevelIndex = 0;
	private int[] remainingGearCounts;

	private Motor motor;
	private List<Target> targets = new List<Target>();
	private Node uiInstance;
	private NotificationManager _notify;

	private float DistXZ(Vector3 a, Vector3 b)
	{
		return new Vector2(a.X - b.X, a.Z - b.Z).Length();
	}
public void LoadLevelByIndex(int index)
{
	if (levels == null || index < 0 || index >= levels.Length) return;
	currentLevelIndex = index;
	var scene = levels[index].levelScene;
	if (scene != null)
		GetTree().ChangeSceneToPacked(scene);
}
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
		foreach (Node node in GetChildren())
			if (node is Gear)
				node.QueueFree();

		var scene = GetTree().CurrentScene;
		if (scene == null) return;

		motor = scene.GetNodeOrNull<Motor>("Motor");
		_notify = scene.GetNodeOrNull<NotificationManager>("Notification");

		targets.Clear();
		foreach (Node node in scene.GetChildren())
			if (node is Target t)
				targets.Add(t);

		if (motor == null || targets.Count == 0) return;

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

	public void ShowNotification(string msg)
	{
		_notify?.Show(msg);
	}

	public void OnTargetActivated()
	{
		foreach (var t in targets)
			if (!t.Activated) return;
		CompleteLevel();
	}

	public void CompleteLevel()
	{
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

	private bool AreCompatible(GearType a, GearType b)
	{
		if (a == null || b == null) return true;
		if (a.compatibleWith == null || a.compatibleWith.Length == 0) return true;
		foreach (var name in a.compatibleWith)
			if (name == b.gearName) return true;
		return false;
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
						ShowNotification("Шестерёнок не осталось!");
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
			if (node is Gear gear && gear.PlacedOnAxis == axis)
			{
				motor.SetProcess(false);
				motor.Children.Clear();
				foreach (Node n in GetChildren())
					if (n is Gear g && IsInstanceValid(g) && g != gear)
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
				gear.PlacedOnAxis = null;
				// Убираем из дерева сразу — чтобы не попала в Recalculate
				RemoveChild(gear);
				gear.QueueFree();
				CallDeferred(nameof(ReenableMotorAndRecalculate));
				return;
			}
		}
		GD.PrintErr("Шестерёнка на оси не найдена!");
	}

	private void ReenableMotorAndRecalculate()
	{
		motor.SetProcess(true);
		Recalculate();
	}

	private void UpdateButtonText(Button btn, int index)
	{
		var type = currentLevel.availableGearTypes[index];
		btn.Text = $"{type.gearName}\n{remainingGearCounts[index]}";
	}

	private void _on_button_pressed(int gearIndex)
	{
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

		// Проверка оверлапа с другими шестерёнками
		foreach (var g in GetAllGears())
		{
			float dist = targetPos.DistanceTo(g.GlobalPosition);
			float minDist = (newRadius + g.Radius) - 0.1f;

			var checkAxisParent = SelectedAxis.GetParent<Node3D>();
			if (checkAxisParent != null)
			{
				float dot = Mathf.Abs(checkAxisParent.GlobalBasis.Column1.Normalized().Dot(g.initialBasis.Column1.Normalized()));
				minDist *= Mathf.Lerp(0.00001f, 1.0f, dot);
			}

			if (dist < minDist)
			{
				ShowNotification("Место занято другой шестерёнкой!");
				return;
			}
		}

		float distMotor = targetPos.DistanceTo(motor.GlobalPosition);
		if (distMotor < (newRadius + motor.Radius) - 0.1f)
		{
			ShowNotification("Место занято мотором!");
			return;
		}

		foreach (var t in targets)
		{
			float distT = targetPos.DistanceTo(t.GlobalPosition);
			if (distT < (newRadius + t.Radius) - 0.1f)
			{
				ShowNotification("Место занято целевой шестерёнкой!");
				return;
			}
		}

		// Проверка совместимости
foreach (var g in GetAllGears())
{
	float dist = targetPos.DistanceTo(g.GlobalPosition);
	var checkAxisParent = SelectedAxis.GetParent<Node3D>();
	if (checkAxisParent == null) continue;

	float dot = Mathf.Abs(checkAxisParent.GlobalBasis.Column1.Normalized().Dot(g.initialBasis.Column1.Normalized()));
	float expected = newRadius + g.Radius;
	bool wouldConnect;

	if (dot < 0.05f) // перпендикулярные
		wouldConnect = dist >= expected * 0.1f && dist <= expected * 0.7f;
	else // параллельные
		wouldConnect = dist >= expected * 0.5f && Mathf.Abs(dist - expected) < 0.2f;

	if (wouldConnect)
	{
		if (!AreCompatible(SelectedGearConfig, g.config) || !AreCompatible(g.config, SelectedGearConfig))
		{
			ShowNotification("Шестерёнки несовместимы!");
			return;
		}
	}
}

		var gear = SelectedGearConfig.scenePrefab.Instantiate<Gear>();
		gear.config = SelectedGearConfig;
		gear.Radius = SelectedGearConfig.Radius;
		gear.ToothCount = SelectedGearConfig.ToothCount;

		AddChild(gear);
		gear.GlobalPosition = targetPos;

		var axisParent = SelectedAxis.GetParent<Node3D>();
		if (axisParent != null)
		{
			gear.GlobalBasis = axisParent.GlobalBasis;
			gear.initialBasis = gear.GlobalBasis;
		}

		float distToMotor = targetPos.DistanceTo(motor.GlobalPosition);
		if (Mathf.Abs(distToMotor - (newRadius + motor.Radius)) < 0.2f)
			gear.SnapPhaseWithMotor(motor);
		else
		{
			foreach (var g in GetAllGears())
			{
				if (g == gear) continue;
				float dist = targetPos.DistanceTo(g.GlobalPosition);
				if (Mathf.Abs(dist - (newRadius + g.Radius)) < 0.2f)
				{
					gear.SnapPhaseWithGear(g);
					break;
				}
			}
		}

		SelectedAxis.HasGear = true;
		gear.PlacedOnAxis = SelectedAxis;

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

	private void Recalculate()
	{
		var gears = GetAllGears();
		if (targets.Count == 0)
		{
			GD.PrintErr("No targets found!");
			return;
		}
		PhysicsEngine.BuildGraph(motor, gears, targets, this);
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

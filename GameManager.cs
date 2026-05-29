using Godot;
using System.Collections.Generic;

public partial class GameManager : Node
{
	public static Axis SelectedAxis;
	public static GearType SelectedGearConfig;
	public float GetTime() => _time;
	public int GetMoves() => _moves;

	[Export] public Level[] levels;

	private Level currentLevel;
	private int currentLevelIndex = 0;
	private int[] remainingGearCounts;

	private Motor motor;
	public List<Target> targets = new List<Target>();
	private Node uiInstance;
	private NotificationManager _notify;

	private float _time = 0f;
	private int _moves = 0;
	private bool _removedGear = false;
	private int _restartCount = 0;
	private int _lastRestartedLevel = -1;

	// Данные для показа результатов после катсцены
	public static int PendingLevelIndex = -1;
	public static float PendingTime = 0;
	public static int PendingMoves = 0;
	public static int PendingStars = 0;
	public static int PendingTotalLevels = 0;

	public void LoadLevelByIndex(int index)
	{
		if (levels == null || index < 0 || index >= levels.Length) return;
		currentLevelIndex = index;
		var scene = levels[index].levelScene;
		if (scene != null)
			GetTree().ChangeSceneToPacked(scene);
	}

	public override void _Process(double delta)
	{
		if (motor != null && motor.IsInsideTree())
			_time += (float)delta;
	}

	public override void _Ready()
	{
		SettingsManager.Load();
		SettingsManager.Apply();
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
		SoundManager.Instance?.StopSpin();
		_time = 0f;
		_moves = 0;
		_removedGear = false;
		foreach (Node node in GetChildren())
			if (node is Gear)
				node.QueueFree();

		var scene = GetTree().CurrentScene;
		if (scene == null) return;

		motor  = scene.GetNodeOrNull<Motor>("Motor");
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

		if (SaveSystem.CurrentUser != null)
		{
			for (int i = 0; i < levels.Length; i++)
			{
				if (i == 0) { levels[i].isUnlocked = true; continue; }
				if (SaveSystem.CurrentUser.LevelResults.ContainsKey(i - 1))
					levels[i].isUnlocked = SaveSystem.CurrentUser.LevelResults[i - 1].BestStars >= 1;
				else
					levels[i].isUnlocked = false;
			}
		}

		currentLevel = levels[currentLevelIndex];

		var uiManager = GetNodeOrNull<UIManager>("/root/UIManager");
		if (uiManager == null) { GD.PrintErr("UIManager не найден!"); return; }

		uiManager.Show("hud");
		uiInstance = uiManager.GetCurrentScreen();

		SetupLevel();
	}

	public void ShowNotification(string msg, string key = "") => _notify?.Show(msg, key);

	public void OnTargetActivated()
	{
		foreach (var t in targets)
			if (!t.Activated) return;

		var animations = GetTree().GetNodesInGroup("LevelAnimation");
		foreach (Node node in animations)
			if (node is LevelAnimation anim)
				anim.Activate();

		CompleteLevel();
	}

	public int CalculateStars(float time, int moves)
	{
		if (currentLevel == null) return 0;
		var timeLimits = currentLevel.StarTimeLimits;
		var moveLimits = currentLevel.StarMoveLimits;

		for (int i = 4; i >= 0; i--)
			if (time <= timeLimits[i] && moves <= moveLimits[i])
				return i + 1;

		return 0;
	}

	public void CompleteLevel()
	{
		SoundManager.Instance?.StopSpin();
		int stars = CalculateStars(_time, _moves);
		SaveSystem.SaveLevelResult(currentLevelIndex, _time, _moves, stars);
		if (stars >= 1 && currentLevelIndex + 1 < levels.Length)
			levels[currentLevelIndex + 1].isUnlocked = true;

		if (currentLevelIndex == 0)
		{
			if (SaveSystem.UnlockAchievement("first_level"))
				ShowNotification(Tr("ACH_FIRST_LEVEL"), "first_level");
		}
		else
		{
			if (!_removedGear)
				if (SaveSystem.UnlockAchievement("no_remove"))
					ShowNotification(Tr("ACH_NO_REMOVE"), "no_remove");

			if (_time < 15f)
				if (SaveSystem.UnlockAchievement("speed_run"))
					ShowNotification(Tr("ACH_SPEED_RUN"), "speed_run");

			if (SaveSystem.CurrentUser != null)
			{
				bool allDone = true;
				for (int i = 0; i < levels.Length; i++)
					if (!SaveSystem.CurrentUser.LevelResults.ContainsKey(i))
						{ allDone = false; break; }
				if (allDone)
					if (SaveSystem.UnlockAchievement("game_complete"))
						ShowNotification(Tr("ACH_GAME_COMPLETE"), "game_complete");
			}

			bool allUsed = true;
			for (int i = 0; i < remainingGearCounts.Length; i++)
				if (remainingGearCounts[i] > 0) { allUsed = false; break; }
			if (allUsed)
				if (SaveSystem.UnlockAchievement("all_gears"))
					ShowNotification(Tr("ACH_ALL_GEARS"), "all_gears");

			if (stars == 5)
				if (SaveSystem.UnlockAchievement("five_stars"))
					ShowNotification(Tr("ACH_FIVE_STARS"), "five_stars");

			if (SaveSystem.CurrentUser != null)
			{
				bool allMax = true;
				for (int i = 0; i < levels.Length; i++)
				{
					if (!SaveSystem.CurrentUser.LevelResults.ContainsKey(i) ||
						SaveSystem.CurrentUser.LevelResults[i].BestStars < 5)
						{ allMax = false; break; }
				}
				if (allMax)
					if (SaveSystem.UnlockAchievement("all_stars"))
						ShowNotification(Tr("ACH_ALL_STARS"), "all_stars");
			}
		}

		float capturedTime  = _time;
		int   capturedMoves = _moves;

		// После 11го уровня (индекс 10) — сначала результаты, потом катсцена через кнопку
		if (currentLevelIndex == 10)
		{
			PendingLevelIndex   = currentLevelIndex;
			PendingTime         = capturedTime;
			PendingMoves        = capturedMoves;
			PendingStars        = stars;
			PendingTotalLevels  = levels.Length;

			var cutsceneUiManager = GetNodeOrNull<UIManager>("/root/UIManager");
			if (cutsceneUiManager == null) return;
			cutsceneUiManager.Show("level_complete");
			var screen = cutsceneUiManager.GetCurrentScreen();
			if (screen is LevelCompleteScreen lcs)
				lcs.SetupWithCutscene(currentLevelIndex, levels.Length, capturedTime, capturedMoves, stars);
			return;
		}

		var timer = GetTree().CreateTimer(2.0f);
		timer.Timeout += () =>
		{
			var uiManager = GetNodeOrNull<UIManager>("/root/UIManager");
			if (uiManager == null) return;
			if (levels == null) return;
			uiManager.Show("level_complete");
			var screen = uiManager.GetCurrentScreen();
			if (screen is LevelCompleteScreen lcs)
				lcs.Setup(currentLevelIndex, levels.Length, capturedTime, capturedMoves, stars);
			else
				GD.PrintErr($"screen type={screen?.GetType().Name}");
		};
	}

	public void ShowPendingResults()
	{
		if (PendingLevelIndex < 0) return;
		var uiManager = GetNodeOrNull<UIManager>("/root/UIManager");
		if (uiManager == null) return;
		uiManager.Show("level_complete");
		var screen = uiManager.GetCurrentScreen();
		if (screen is LevelCompleteScreen lcs)
			lcs.Setup(PendingLevelIndex, PendingTotalLevels, PendingTime, PendingMoves, PendingStars);
		PendingLevelIndex = -1;
	}

	public void RestartLevel()
	{
		if (_lastRestartedLevel != currentLevelIndex)
		{
			_restartCount = 0;
			_lastRestartedLevel = currentLevelIndex;
		}
		_restartCount++;
		if (_restartCount >= 3 && currentLevelIndex != 0)
			if (SaveSystem.UnlockAchievement("persistent"))
				ShowNotification(Tr("ACH_PERSISTENT"), "persistent");

		var scene = levels[currentLevelIndex].levelScene;
		if (scene != null)
			GetTree().ChangeSceneToPacked(scene);
	}

	private void SetupLevel()
	{
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
		if (currentLevel.availableGearTypes == null) return;

		for (int i = 0; i < currentLevel.availableGearTypes.Length; i++)
		{
			int index = i;
			var btn = uiInstance.GetNodeOrNull<TextureButton>($"UI/Panel/Button{i}");
			if (btn != null)
			{
				UpdateButtonLabel(btn, index);
				btn.Pressed += () =>
				{
					if (remainingGearCounts[index] <= 0)
					{
						ShowNotification(Tr("NO_GEARS_LEFT"));
						return;
					}
					SelectedGearConfig = currentLevel.availableGearTypes[index];
					_on_button_pressed(index);
				};
			}
		}

		var removeBtn = uiInstance.GetNodeOrNull<TextureButton>("UI/Panel/RemoveButton");
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
						var btn = uiInstance.GetNodeOrNull<TextureButton>($"UI/Panel/Button{i}");
						if (btn != null) UpdateButtonLabel(btn, i);
						break;
					}
				}

				axis.HasGear      = false;
				gear.PlacedOnAxis = null;
				_removedGear      = true;
				RemoveChild(gear);
				gear.QueueFree();
				_moves++;
				SoundManager.Instance?.PlayGearRemove();
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

	private void UpdateButtonLabel(TextureButton btn, int index)
	{
		var label = btn.GetNodeOrNull<Label>("Label");
		if (label != null)
		{
			var type = currentLevel.availableGearTypes[index];
			label.Text = $"{type.gearName}\n{remainingGearCounts[index]}";
		}
	}

	private void _on_button_pressed(int gearIndex)
	{
		if (SelectedAxis == null || SelectedAxis.HasGear) return;
		if (SelectedGearConfig == null) return;
		if (SelectedGearConfig.scenePrefab == null) return;

		Vector3 targetPos = SelectedAxis.GlobalPosition;
		float newRadius   = SelectedGearConfig.Radius;

		foreach (var g in GetAllGears())
		{
			float dist    = targetPos.DistanceTo(g.GlobalPosition);
			float minDist = (newRadius + g.Radius) - 0.1f;

			var checkAxisParent = SelectedAxis.GetParent<Node3D>();
			if (checkAxisParent != null)
			{
				float dot = Mathf.Abs(checkAxisParent.GlobalBasis.Column1.Normalized()
					.Dot(g.initialBasis.Column1.Normalized()));
				minDist *= Mathf.Lerp(0.00001f, 1.0f, dot);
			}

			if (dist < minDist)
			{
				ShowNotification(Tr("NO_SPACE"));
				return;
			}
		}

		float distMotor = targetPos.DistanceTo(motor.GlobalPosition);
		if (distMotor < (newRadius + motor.Radius) - 0.1f)
		{
			ShowNotification(Tr("OCCUPIED_MOTOR"));
			return;
		}

		foreach (var t in targets)
		{
			float distT = targetPos.DistanceTo(t.GlobalPosition);
			if (distT < (newRadius + t.Radius) - 0.1f)
			{
				ShowNotification(Tr("OCCUPIED_TARGET"));
				return;
			}
		}

		if (SelectedGearConfig.gearName == "Big")
		{
			float distToMotor   = targetPos.DistanceTo(motor.GlobalPosition);
			float expectedMotor = SelectedGearConfig.Radius + motor.Radius;
			if (Mathf.Abs(distToMotor - expectedMotor) < 0.3f)
			{
				ShowNotification(Tr("BIG_NO_MOTOR"));
				return;
			}
			foreach (var t in targets)
			{
				float distToTarget   = targetPos.DistanceTo(t.GlobalPosition);
				float expectedTarget = SelectedGearConfig.Radius + t.Radius;
				if (Mathf.Abs(distToTarget - expectedTarget) < 0.3f)
				{
					ShowNotification(Tr("BIG_NO_TARGET"));
					return;
				}
			}
		}

		foreach (var g in GetAllGears())
		{
			float dist = targetPos.DistanceTo(g.GlobalPosition);
			var checkAxisParent = SelectedAxis.GetParent<Node3D>();
			if (checkAxisParent == null) continue;

			float dot      = Mathf.Abs(checkAxisParent.GlobalBasis.Column1.Normalized()
				.Dot(g.initialBasis.Column1.Normalized()));
			float expected = newRadius + g.Radius;
			bool wouldConnect;

			if (dot < 0.05f)
				wouldConnect = dist >= expected * 0.3f && dist <= expected * 0.7f;
			else
				wouldConnect = dist >= expected * 0.5f && Mathf.Abs(dist - expected) < 0.2f;

			if (wouldConnect)
			{
				if (!AreCompatible(SelectedGearConfig, g.config) || !AreCompatible(g.config, SelectedGearConfig))
				{
					ShowNotification(Tr("INCOMPATIBLE"));
					return;
				}
			}
		}

		var gear = SelectedGearConfig.scenePrefab.Instantiate<Gear>();
		gear.config     = SelectedGearConfig;
		gear.Radius     = SelectedGearConfig.Radius;
		gear.ToothCount = SelectedGearConfig.ToothCount;

		AddChild(gear);
		gear.GlobalPosition = targetPos;

		var axisParent = SelectedAxis.GetParent<Node3D>();
		if (axisParent != null)
		{
			gear.GlobalBasis  = axisParent.GlobalBasis;
			gear.initialBasis = gear.GlobalBasis;
		}

		SelectedAxis.HasGear  = true;
		gear.PlacedOnAxis     = SelectedAxis;

		for (int i = 0; i < currentLevel.availableGearTypes.Length; i++)
		{
			if (currentLevel.availableGearTypes[i] == SelectedGearConfig)
			{
				remainingGearCounts[i]--;
				var btn = uiInstance.GetNodeOrNull<TextureButton>($"UI/Panel/Button{i}");
				if (btn != null) UpdateButtonLabel(btn, i);
				break;
			}
		}
		_moves++;
		SoundManager.Instance?.PlayGearPlace();
		Recalculate();
	}

	public void CheckChainAchievement(List<Gear> gears)
	{
		foreach (var g in gears)
		{
			if (g.Children.Count >= 4)
			{
				if (SaveSystem.UnlockAchievement("chain_master"))
					ShowNotification(Tr("ACH_CHAIN_MASTER"), "chain_master");
				return;
			}
		}
	}

	private void Recalculate()
	{
		var gears = GetAllGears();
		if (targets.Count == 0) return;
		PhysicsEngine.BuildGraph(motor, gears, targets, this);
		CheckChainAchievement(gears);

		foreach (var g in motor.Children)
			if (IsInstanceValid(g))
				g.UpdateRotation();

		if (motor.Children.Count > 0)
			SoundManager.Instance?.StartSpin();
		else
			SoundManager.Instance?.StopSpin();
	}

	public void LoadNextLevel()
	{
		currentLevelIndex++;
		if (currentLevelIndex < levels.Length)
		{
			var next = levels[currentLevelIndex];
			if (next.levelScene != null)
				GetTree().ChangeSceneToPacked(next.levelScene);
		}
	}

	public List<Gear> GetAllGears()
	{
		var gears = new List<Gear>();
		foreach (Node node in GetChildren())
			if (node is Gear g && IsInstanceValid(g))
				gears.Add(g);
		return gears;
	}

	public void RestoreUnlocks()
	{
		if (SaveSystem.CurrentUser == null || levels == null) return;
		for (int i = 0; i < levels.Length; i++)
		{
			if (i == 0) { levels[i].isUnlocked = true; continue; }
			if (SaveSystem.CurrentUser.LevelResults.ContainsKey(i - 1))
				levels[i].isUnlocked = SaveSystem.CurrentUser.LevelResults[i - 1].BestStars >= 1;
			else
				levels[i].isUnlocked = false;
		}
	}
}

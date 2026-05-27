using Godot;

public partial class LevelCompleteScreen : CanvasLayer
{
	public override void _Ready()
	{
		Layer = 20;
	}

	public void Setup(int currentIndex, int totalLevels, float time, int moves, int stars)
	{
		var nextBtn = GetNodeOrNull<TextureButton>("Panel/nextButton");
if (nextBtn != null)
{
	if (currentIndex + 1 < totalLevels)
	{
		nextBtn.Visible = true;
		nextBtn.Pressed += () =>
		{
			SoundManager.Instance?.PlayClick();
			QueueFree();
			GetTree().ChangeSceneToFile("res://UI/LevelSelect.tscn");
		};
	}
	else
	{
		// Последний уровень — показываем кнопку но ведём в главное меню
		nextBtn.Visible = true;
		var nextLbl = nextBtn.GetNodeOrNull<Label>("Button2Label");
		if (nextLbl != null) nextLbl.Text = Tr("MAIN_MENU");
		nextBtn.Pressed += () =>
		{
			SoundManager.Instance?.PlayClick();
			QueueFree();
			GetTree().ChangeSceneToFile("res://main.tscn");
		};
	}
}

		var levelsBtn = GetNodeOrNull<TextureButton>("Panel/LevelsButton");
if (levelsBtn != null)
{
	levelsBtn.Visible = true;
	levelsBtn.Pressed += () =>
	{
		SoundManager.Instance?.PlayClick();
		GetTree().ChangeSceneToFile("res://UI/LevelSelect.tscn");
	};
}

		var restartBtn = GetNodeOrNull<TextureButton>("Panel/RestartButton");
		if (restartBtn != null)
			restartBtn.Pressed += () =>
			{
				SoundManager.Instance?.PlayClick();
				var gm = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
				gm?.RestartLevel();
			};

		var timeLabel  = GetNodeOrNull<Label>("Panel/TimeLabel");
		var movesLabel = GetNodeOrNull<Label>("Panel/MovesLabel");
		var starsLabel = GetNodeOrNull<Label>("Panel/StarsLabel");

		if (timeLabel  != null) timeLabel.Text  = $"{Tr("TIME_LABEL")} {(int)time / 60}:{(time % 60):00}";
		if (movesLabel != null) movesLabel.Text = $"{Tr("MOVES_LABEL")} {moves}";
		if (starsLabel != null) starsLabel.Text = new string('★', stars) + new string('☆', 5 - stars);

		var nextLabel    = GetNodeOrNull<Label>("Panel/Button2Label");
		if (nextLabel != null) nextLabel.Text = Tr("CH_LEVEL");
	}
}

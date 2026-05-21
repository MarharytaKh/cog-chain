using Godot;

public partial class LevelCompleteScreen : CanvasLayer
{
	public override void _Ready()
	{
		Layer = 20;
	}

	public void Setup(int currentIndex, int totalLevels, float time, int moves, int stars)
	{
		// Кнопка следующего уровня → выбор уровней
		var nextBtn = GetNodeOrNull<TextureButton>("Panel/nextButton");
		if (nextBtn != null)
		{
			nextBtn.Visible = currentIndex + 1 < totalLevels;
			nextBtn.Pressed += () =>
			{
				QueueFree();
				GetTree().ChangeSceneToFile("res://UI/LevelSelect.tscn");
			};
		}

		var levelsBtn = GetNodeOrNull<TextureButton>("Panel/LevelsButton");
		if (levelsBtn != null)
			levelsBtn.Pressed += () => GetTree().ChangeSceneToFile("res://UI/LevelSelect.tscn");

		var restartBtn = GetNodeOrNull<TextureButton>("Panel/RestartButton");
		if (restartBtn != null)
			restartBtn.Pressed += () =>
			{
				var gm = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
				gm?.RestartLevel();
			};

		// Время и ходы
		var timeLabel  = GetNodeOrNull<Label>("Panel/TimeLabel");
		var movesLabel = GetNodeOrNull<Label>("Panel/MovesLabel");
		var starsLabel = GetNodeOrNull<Label>("Panel/StarsLabel");

		if (timeLabel  != null) timeLabel.Text  = $"Время: {(int)time / 60}:{(time % 60):00}";
		if (movesLabel != null) movesLabel.Text = $"Ходов: {moves}";
		if (starsLabel != null) starsLabel.Text = new string('★', stars) + new string('☆', 5 - stars);
	}
}

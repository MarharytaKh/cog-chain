using Godot;
public partial class LevelCompleteScreen : CanvasLayer
{
	public override void _Ready()
	{
		Layer = 20;
		GD.Print("LevelCompleteScreen Ready");
	}

	public void Setup(int currentIndex, int totalLevels, float time, int moves)
	{
		GD.Print("Setup вызван");
		var nextBtn = GetNodeOrNull<TextureButton>("Panel/nextButton");
		GD.Print($"NextButton найден: {nextBtn != null}");
		if (nextBtn != null)
		{
			nextBtn.Visible = currentIndex + 1 < totalLevels;
			nextBtn.Pressed += () => {
				var gm = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
				gm?.LoadNextLevel();
			};
		}

		var levelsBtn = GetNodeOrNull<TextureButton>("Panel/LevelsButton");
		if (levelsBtn != null)
			levelsBtn.Pressed += () => GetTree().ChangeSceneToFile("res://UI/LevelSelect.tscn");

		var restartBtn = GetNodeOrNull<TextureButton>("Panel/RestartButton");
		if (restartBtn != null)
			restartBtn.Pressed += () => {
				var gm = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
				gm?.RestartLevel();
			};

		var timeLabel = GetNodeOrNull<Label>("Panel/TimeLabel");
		var movesLabel = GetNodeOrNull<Label>("Panel/MovesLabel");

		if (timeLabel != null)
			timeLabel.Text = $"Время: {(int)time / 60}:{(time % 60):00}";
		if (movesLabel != null)
			movesLabel.Text = $"Ходов: {moves}";
	}
}

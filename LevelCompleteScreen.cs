using Godot;
public partial class LevelCompleteScreen : CanvasLayer
{
	public override void _Ready()
	{
		Layer = 20;
		GD.Print("LevelCompleteScreen Ready");
	}

	public void Setup(int currentIndex, int totalLevels)
	{
		GD.Print("Setup вызван");
		var nextBtn = GetNodeOrNull<Button>("Panel/nextButton");
		GD.Print($"NextButton найден: {nextBtn != null}");
		if (nextBtn != null)
		{
			nextBtn.Visible = currentIndex + 1 < totalLevels;
			nextBtn.Pressed += () => {
				var gm = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
				gm?.LoadNextLevel();
			};
		}
		var levelsBtn = GetNodeOrNull<Button>("Panel/LevelsButton");
		if (levelsBtn != null)
			levelsBtn.Pressed += () => GetTree().ChangeSceneToFile("res://UI/main.tscn");
		var restartBtn = GetNodeOrNull<Button>("Panel/RestartButton");
		if (restartBtn != null)
			restartBtn.Pressed += () => {
				var gm = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
				gm?.RestartLevel();
			};
	}
}

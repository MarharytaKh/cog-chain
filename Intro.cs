using Godot;

public partial class Intro : Control
{
	[Export] public string NextScene = "res://account/LoginScreen.tscn";

	public override void _Ready()
	{
		SoundManager.Instance?.StopSpin();
		var video = GetNode<VideoStreamPlayer>("VideoStreamPlayer");
		video.Finished += () =>
		{
			SoundManager.Instance?.StartMusic();
			// Если есть ожидающие результаты — показываем их
			if (GameManager.PendingLevelIndex >= 0)
			{
				var gm = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
				gm?.ShowPendingResults();
				return;
			}
			GetTree().ChangeSceneToFile(NextScene);
		};
		video.Play();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventScreenTouch || @event is InputEventMouseButton)
		{
			SoundManager.Instance?.StartMusic();
			if (GameManager.PendingLevelIndex >= 0)
			{
				var gm = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
				gm?.ShowPendingResults();
				return;
			}
			GetTree().ChangeSceneToFile(NextScene);
		}
	}
}

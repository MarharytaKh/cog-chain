using Godot;

public partial class PauseMenu : CanvasLayer
{
	public override void _Ready()
	{
		Layer = 30;
		ProcessMode = ProcessModeEnum.Always;
		GetTree().Paused = true;

		var resumeBtn = GetNode<TextureButton>("Panel/ResumeButton");
		resumeBtn.ProcessMode = ProcessModeEnum.Always;
		resumeBtn.Pressed += () =>
		{
			SoundManager.Instance?.PlayClick();
			GetTree().Paused = false;
			QueueFree();
		};

		var menuBtn = GetNode<TextureButton>("Panel/MenuButton");
		menuBtn.ProcessMode = ProcessModeEnum.Always;
		menuBtn.Pressed += () =>
		{
			SoundManager.Instance?.PlayClick();
			GetTree().Paused = false;
			QueueFree();
			var uiManager = GetNode<UIManager>("/root/UIManager");
			uiManager.Hide();
			GetTree().ChangeSceneToFile("res://main.tscn");
		};
	}
}

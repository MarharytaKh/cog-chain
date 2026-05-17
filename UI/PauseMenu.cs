using Godot;

public partial class PauseMenu : CanvasLayer
{
	public override void _Ready()
	{
		Layer = 30;
		ProcessMode = ProcessModeEnum.Always;
		GetTree().Paused = true;

		GetNode<TextureButton>("Panel/ResumeButton").ProcessMode = ProcessModeEnum.Always;
		GetNode<TextureButton>("Panel/MenuButton").ProcessMode = ProcessModeEnum.Always;

		GetNode<TextureButton>("Panel/ResumeButton").Pressed += () =>
		{
			GetTree().Paused = false;
			QueueFree();
		};

		GetNode<TextureButton>("Panel/MenuButton").Pressed += () =>
		{
			GetTree().Paused = false;
			QueueFree();
			var uiManager = GetNode<UIManager>("/root/UIManager");
			uiManager.Hide();
			GetTree().ChangeSceneToFile("res://main.tscn");
		};
	}
}

using Godot;

public partial class PauseMenu : CanvasLayer
{
public override void _Ready()
{
	Layer = 30;
	ProcessMode = ProcessModeEnum.Always; // ← этот узел работает даже на паузе
	GetTree().Paused = true;

	GetNode<Button>("Panel/ResumeButton").ProcessMode = ProcessModeEnum.Always;
	GetNode<Button>("Panel/MenuButton").ProcessMode = ProcessModeEnum.Always;

	GetNode<Button>("Panel/ResumeButton").Pressed += () =>
	{
		GetTree().Paused = false;
		QueueFree();
	};

	GetNode<Button>("Panel/MenuButton").Pressed += () =>
	{
		GetTree().Paused = false;
		QueueFree();
		var uiManager = GetNode<UIManager>("/root/UIManager");
		uiManager.Hide();
		GetTree().ChangeSceneToFile("res://main.tscn");
	};
}
}

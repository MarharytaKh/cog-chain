using Godot;

public partial class Intro : Control
{
	public override void _Ready()
	{
		var video = GetNode<VideoStreamPlayer>("VideoStreamPlayer");
		video.Finished += () => GetTree().ChangeSceneToFile("res://account/LoginScreen.tscn");
		video.Play();
	}

	public override void _Input(InputEvent @event)
	{
		// Пропуск по тапу
		if (@event is InputEventScreenTouch || @event is InputEventMouseButton)
			GetTree().ChangeSceneToFile("res://account/LoginScreen.tscn");
	}
}

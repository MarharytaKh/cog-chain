using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<TextureButton>("Button").Pressed += () =>
			GetTree().ChangeSceneToFile("res://UI/LevelSelect.tscn");
			GetNode<TextureButton>("LoadButton").Pressed += () =>
	GetTree().ChangeSceneToFile("res://account/SavesScreen.tscn");
	}
	
}

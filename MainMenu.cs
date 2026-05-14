using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<Button>("Button").Pressed += () =>
			GetTree().ChangeSceneToFile("res://UI/LevelSelect.tscn");
			GetNode<Button>("LoadButton").Pressed += () =>
	GetTree().ChangeSceneToFile("res://account/SavesScreen.tscn");
	}
	
}

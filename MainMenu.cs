using Godot;
public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<Button>("Button").Pressed += () =>
			GetTree().ChangeSceneToFile("res://levels/l1.tscn");
	}
}

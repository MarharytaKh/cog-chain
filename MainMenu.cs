using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<TextureButton>("Button").Pressed += () =>
		{
			SoundManager.Instance?.PlayClick();
			GetTree().ChangeSceneToFile("res://UI/LevelSelect.tscn");
		};

		GetNode<TextureButton>("LoadButton").Pressed += () =>
		{
			SoundManager.Instance?.PlayClick();
			GetTree().ChangeSceneToFile("res://account/SavesScreen.tscn");
		};

		GetNode<TextureButton>("AchievementButton").Pressed += () =>
		{
			SoundManager.Instance?.PlayClick();
			GetTree().ChangeSceneToFile("res://UI/AchievementsScreen.tscn");
		};

		GetNode<TextureButton>("SettingsButton").Pressed += () =>
		{
			SoundManager.Instance?.PlayClick();
			GetTree().ChangeSceneToFile("res://UI/SettingsScreen.tscn");
		};
	}
}

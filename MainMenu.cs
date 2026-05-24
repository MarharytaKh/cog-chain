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
		
		
				var newLabel = GetNodeOrNull<Label>("Button/Button2Label");
if (newLabel != null) newLabel.Text = Tr("CH_LEVEL");

var userLabel = GetNodeOrNull<Label>("UserLabel");
if (userLabel != null)
	userLabel.Text = SaveSystem.CurrentUser?.Username ?? "";

		var loadLabel = GetNodeOrNull<Label>("LoadButton/LoadButton2Label");
if (loadLabel != null) loadLabel.Text = Tr("L_SAVE");

var achLabel = GetNodeOrNull<Label>("AchievementButton/LoadButtonAchievements");
if (achLabel != null) achLabel.Text = Tr("ACHIEVEMENTS");

var btnLabel = GetNodeOrNull<Label>("SettingsButton/LabelSettings");
if (btnLabel != null) btnLabel.Text = Tr("SETTINGS");
	}
}

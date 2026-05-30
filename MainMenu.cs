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

		GetNode<TextureButton>("RankingButton2").Pressed += () =>
		{
			SoundManager.Instance?.PlayClick();
			GetTree().ChangeSceneToFile("res://UI/RankingScene.tscn");
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
		var logoutBtn = GetNodeOrNull<TextureButton>("LogoutButton");
		if (logoutBtn != null)
		logoutBtn.Pressed += () =>
{
	SoundManager.Instance?.PlayClick();
	FirebaseManager.IdToken = "";
	FirebaseManager.LocalId = "";
	SaveSystem.Logout();
	// Сбрасываем уровни
	var gm = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
	if (gm?.levels != null)
	{
		gm.levels[0].isUnlocked = true;
		for (int i = 1; i < gm.levels.Length; i++)
			gm.levels[i].isUnlocked = false;
	}
	GetTree().ChangeSceneToFile("res://account/LoginScreen.tscn");
};
		var logoutLabel = GetNodeOrNull<Label>("LogoutButton/LogoutButtonLabel");
		if (logoutLabel != null) logoutLabel.Text = Tr("LOGOUT");

		var rankLabel = GetNodeOrNull<Label>("RankingButton2/LoadButtonRanking");
		if (rankLabel != null) rankLabel.Text = Tr("RANKING");

		var userLabel = GetNodeOrNull<Label>("UserLabel");
		if (userLabel != null) userLabel.Text = SaveSystem.CurrentUser?.Username ?? "";

		var newLabel = GetNodeOrNull<Label>("Button/Button2Label");
		if (newLabel != null) newLabel.Text = Tr("CH_LEVEL");

		var loadLabel = GetNodeOrNull<Label>("LoadButton/LoadButton2Label");
		if (loadLabel != null) loadLabel.Text = Tr("L_SAVE");

		var achLabel = GetNodeOrNull<Label>("AchievementButton/LoadButtonAchievements");
		if (achLabel != null) achLabel.Text = Tr("ACHIEVEMENTS");

		var btnLabel = GetNodeOrNull<Label>("SettingsButton/LabelSettings");
		if (btnLabel != null) btnLabel.Text = Tr("SETTINGS");
	}
}

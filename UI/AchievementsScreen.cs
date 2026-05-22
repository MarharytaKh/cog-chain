using Godot;

public partial class AchievementsScreen : CanvasLayer
{
	private static readonly (string key, string nameKey, string descKey)[] All = {
		("first_level",   "ACH_NAME_FIRST_LEVEL",   "ACH_DESC_FIRST_LEVEL"),
		("chain_master",  "ACH_NAME_CHAIN_MASTER",  "ACH_DESC_CHAIN_MASTER"),
		("no_remove",     "ACH_NAME_NO_REMOVE",     "ACH_DESC_NO_REMOVE"),
		("speed_run",     "ACH_NAME_SPEED_RUN",     "ACH_DESC_SPEED_RUN"),
		("game_complete", "ACH_NAME_GAME_COMPLETE",  "ACH_DESC_GAME_COMPLETE"),
		("all_gears",     "ACH_NAME_ALL_GEARS",     "ACH_DESC_ALL_GEARS"),
		("five_stars",    "ACH_NAME_FIVE_STARS",    "ACH_DESC_FIVE_STARS"),
		("all_stars",     "ACH_NAME_ALL_STARS",     "ACH_DESC_ALL_STARS"),
		("persistent",    "ACH_NAME_PERSISTENT",    "ACH_DESC_PERSISTENT"),
	};

	public override void _Ready()
	{
		Layer = 15;
var backLabel = GetNodeOrNull<Label>("Button2Label");
if (backLabel != null) backLabel.Text = Tr("BACK");

		var backBtn = GetNodeOrNull<TextureButton>("BackButton");
		if (backBtn != null)
			backBtn.Pressed += () =>
			{
				SoundManager.Instance?.PlayClick();
				GetTree().ChangeSceneToFile("res://main.tscn");
			};

		var vbox = GetNodeOrNull<VBoxContainer>("ScrollContainer/VBoxContainer");
		if (vbox == null) return;

		foreach (var (key, nameKey, descKey) in All)
		{
			bool unlocked = SaveSystem.HasAchievement(key);
			var label = new Label();
			label.Text = unlocked
				? $"✅ {Tr(nameKey)} — {Tr(descKey)}"
				: $"🔒 ???";
			label.Modulate = unlocked ? new Color(1, 1, 1) : new Color(0.5f, 0.5f, 0.5f);
			vbox.AddChild(label);
		}
	}
}

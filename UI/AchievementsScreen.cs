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

	[Export] public FontFile ButtonFont;
	[Export] public int ButtonFontSize = 24;
	[Export] public int DescFontSize = 18;
	[Export] public Color FontColor = new Color(1, 1, 1);
	[Export] public Color LockedColor = new Color(0.5f, 0.5f, 0.5f);
	[Export] public Vector2 IconSize = new Vector2(64, 64);
	[Export] public int IconMarginLeft = 60;
	[Export] public Godot.Collections.Array<Texture2D> AchievementIcons = new();
	[Export] public Texture2D RowTexture;
	[Export] public Vector2 RowSize = new Vector2(0, 100);

	public override void _Ready()
	{
		Layer = 15;

		var backLabel = GetNodeOrNull<Label>("Button2Label");
		if (backLabel != null) backLabel.Text = Tr("BACK");

		var nameLabel = GetNodeOrNull<Label>("Name");
		if (nameLabel != null) nameLabel.Text = Tr("ACHIEVEMENTS");

		var textLabel = GetNodeOrNull<Label>("LabelT");
		if (textLabel != null) textLabel.Text = Tr("T_ACHIEVEMENTS");

		var backBtn = GetNodeOrNull<TextureButton>("BackButton");
		if (backBtn != null)
			backBtn.Pressed += () =>
			{
				SoundManager.Instance?.PlayClick();
				GetTree().ChangeSceneToFile("res://main.tscn");
			};

		var vbox = GetNodeOrNull<VBoxContainer>("ScrollContainer/VBoxContainer");
		if (vbox == null) return;

		for (int i = 0; i < All.Length; i++)
		{
			var (key, nameKey, descKey) = All[i];
			bool unlocked = SaveSystem.HasAchievement(key);
			Color color = unlocked ? FontColor : LockedColor;

			var rowBtn = new TextureButton();
			rowBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			rowBtn.IgnoreTextureSize = true;
			rowBtn.StretchMode = TextureButton.StretchModeEnum.Scale;
			rowBtn.CustomMinimumSize = RowSize;
			if (RowTexture != null)
				rowBtn.TextureNormal = RowTexture;

			var row = new HBoxContainer();
			row.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			rowBtn.AddChild(row);

			// Отступ + иконка
			var margin = new MarginContainer();
			margin.AddThemeConstantOverride("margin_left", IconMarginLeft);

			var icon = new TextureRect();
			icon.CustomMinimumSize = IconSize;
			icon.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
			icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
			icon.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;
			icon.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
			if (AchievementIcons != null && i < AchievementIcons.Count && AchievementIcons[i] != null)
				icon.Texture = AchievementIcons[i];
			icon.Modulate = unlocked ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.4f);
			margin.AddChild(icon);
			row.AddChild(margin);

			// Текст
			var textBox = new VBoxContainer();
			textBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			textBox.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;

			var titleLabel = new Label();
			titleLabel.Text = unlocked ? Tr(nameKey) : "???";
			titleLabel.Modulate = color;
			if (ButtonFont != null) titleLabel.AddThemeFontOverride("font", ButtonFont);
			titleLabel.AddThemeFontSizeOverride("font_size", ButtonFontSize);
			textBox.AddChild(titleLabel);

			var descLabel = new Label();
			descLabel.Text = unlocked ? Tr(descKey) : "🔒";
			descLabel.Modulate = color;
			descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
			if (ButtonFont != null) descLabel.AddThemeFontOverride("font", ButtonFont);
			descLabel.AddThemeFontSizeOverride("font_size", DescFontSize);
			textBox.AddChild(descLabel);

			row.AddChild(textBox);
			vbox.AddChild(rowBtn);
		}
	}
}

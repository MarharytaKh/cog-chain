using Godot;

public partial class LevelSelect : CanvasLayer
{
	[Export] public Texture2D ButtonTexture;
	[Export] public FontFile ButtonFont;
	[Export] public int ButtonFontSize = 24;
	[Export] public Vector2 ButtonSize = new Vector2(160, 80);
	[Export] public Texture2D ButtonTextureHover;
[Export] public Texture2D ButtonTexturePressed;

	public override void _Ready()
	{
		Layer = 15;

var backLabel = GetNodeOrNull<Label>("Panel/Button2Label");
if (backLabel != null) backLabel.Text = Tr("BACK");


		var backBtn = GetNodeOrNull<TextureButton>("Panel/BackButton");
		if (backBtn != null)
			backBtn.Pressed += () =>
			{
				SoundManager.Instance?.PlayClick();
				GetTree().ChangeSceneToFile("res://main.tscn");
			};

		var grid = GetNodeOrNull<GridContainer>("Panel/ScrollContainer/GridContainer");
		if (grid == null) return;

		var gm = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gm?.levels == null) return;

		for (int i = 0; i < gm.levels.Length; i++)
		{
			int index = i;
			var level = gm.levels[i];

			var btn = new TextureButton();
			btn.CustomMinimumSize = ButtonSize;
			btn.IgnoreTextureSize = true;
			btn.StretchMode = TextureButton.StretchModeEnum.Scale;

			if (ButtonTexture != null)
				btn.TextureNormal = ButtonTexture;
				if (ButtonTextureHover != null)
	btn.TextureHover = ButtonTextureHover;
if (ButtonTexturePressed != null)
	btn.TexturePressed = ButtonTexturePressed;

			var label = new Label();
			label.Text = $"{Tr("LEVEL_NUM")} {i + 1}";
			label.HorizontalAlignment = HorizontalAlignment.Center;
			label.VerticalAlignment = VerticalAlignment.Center;
			label.SetAnchorsPreset(Control.LayoutPreset.FullRect);

			if (ButtonFont != null)
				label.AddThemeFontOverride("font", ButtonFont);
			label.AddThemeFontSizeOverride("font_size", ButtonFontSize);

			if (!level.isUnlocked)
			{
				btn.Disabled = true;
				label.Text += "  🔒";
			}

			btn.AddChild(label);
			btn.Pressed += () =>
			{
				SoundManager.Instance?.PlayClick();
				gm.LoadLevelByIndex(index);
			};
			grid.AddChild(btn);
		}
	}
}

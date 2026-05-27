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
		var sc = GetNodeOrNull<ScrollContainer>("Panel/ScrollContainer");
		if (sc != null)
		{
			sc.GuiInput += (InputEvent evt) =>
			{
				if (evt is InputEventMouseButton mb)
				{
					if (mb.ButtonIndex == MouseButton.WheelUp)
						sc.ScrollVertical -= 120;
					else if (mb.ButtonIndex == MouseButton.WheelDown)
						sc.ScrollVertical += 120;
				}
				else if (evt is InputEventScreenDrag drag)
				{
					sc.ScrollVertical -= (int)drag.Relative.Y;
				}
				else if (evt is InputEventPanGesture pan)
				{
					sc.ScrollVertical += (int)(pan.Delta.Y * 30);
				}
			};
		}

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
			btn.MouseFilter = Control.MouseFilterEnum.Pass;
			if (ButtonTexture != null)        btn.TextureNormal  = ButtonTexture;
			if (ButtonTextureHover != null)   btn.TextureHover   = ButtonTextureHover;
			if (ButtonTexturePressed != null) btn.TexturePressed = ButtonTexturePressed;
			var vbox = new VBoxContainer();
			vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			vbox.Alignment = BoxContainer.AlignmentMode.Center;
			vbox.MouseFilter = Control.MouseFilterEnum.Pass;
			var label = new Label();
			label.Text = $"{Tr("LEVEL_NUM")} {i + 1}";
			label.HorizontalAlignment = HorizontalAlignment.Center;
			label.MouseFilter = Control.MouseFilterEnum.Pass;
			if (ButtonFont != null) label.AddThemeFontOverride("font", ButtonFont);
			label.AddThemeFontSizeOverride("font_size", ButtonFontSize);
			int bestStars = SaveSystem.GetBestStars(i);
			var starsLabel = new Label();
			starsLabel.Text = bestStars > 0
				? new string('★', bestStars) + new string('☆', 5 - bestStars)
				: "";
			starsLabel.HorizontalAlignment = HorizontalAlignment.Center;
			starsLabel.MouseFilter = Control.MouseFilterEnum.Pass;
			if (ButtonFont != null) starsLabel.AddThemeFontOverride("font", ButtonFont);
			starsLabel.AddThemeFontSizeOverride("font_size", ButtonFontSize - 6);
			if (!level.isUnlocked)
			{
				btn.Disabled = true;
				label.Text += "  🔒";
			}
			vbox.AddChild(label);
			vbox.AddChild(starsLabel);
			btn.AddChild(vbox);
			btn.Pressed += () =>
			{
				SoundManager.Instance?.PlayClick();
				gm.LoadLevelByIndex(index);
			};
			grid.AddChild(btn);
		}
	}
}

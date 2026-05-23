using Godot;
 
public partial class SavesScreen : CanvasLayer
{
	[Export] public Texture2D ButtonTexture;
	[Export] public FontFile ButtonFont;
	[Export] public int ButtonFontSize = 24;
	[Export] public Vector2 ButtonSize = new Vector2(400, 60);
	[Export] public Color FontColor = new Color(1, 1, 1);
 
	public override void _Ready()
	{
		GetNode<TextureButton>("Panel/BackButton").Pressed += () =>
		{
			SoundManager.Instance?.PlayClick();
			GetTree().ChangeSceneToFile("res://main.tscn");
		};
		
				var backLabel = GetNodeOrNull<Label>("Panel/Label");
if (backLabel != null) backLabel.Text = Tr("BACK");

				var textLabel = GetNodeOrNull<Label>("LabelT");
if (textLabel != null) textLabel.Text = Tr("TEXT_SAVES");

				var nameLabel = GetNodeOrNull<Label>("Name");
if (nameLabel != null) nameLabel.Text = Tr("NAME_SAVE");

 
		var vbox = GetNode<VBoxContainer>("Panel/ScrollContainer/VBoxContainer");
		if (SaveSystem.CurrentUser == null) return;
 
		foreach (var lvl in SaveSystem.CurrentUser.LevelResults)
		{
			if (!lvl.Value.Completed) continue;
 
			int index    = lvl.Key;
			var result   = lvl.Value;
			int mins     = (int)result.BestTime / 60;
			float secs   = result.BestTime % 60;
			string stars = new string('★', result.BestStars) + new string('☆', 5 - result.BestStars);
 
			var btn = new TextureButton();
			btn.CustomMinimumSize = ButtonSize;
			btn.IgnoreTextureSize = true;
			btn.StretchMode = TextureButton.StretchModeEnum.Scale;
 
			if (ButtonTexture != null)
				btn.TextureNormal = ButtonTexture;
 
			var label = new Label();
			label.Text = $"{Tr("LEVEL_NUM")} {index + 1}   {stars}   ⏱ {mins}:{secs:00}   🔧 {result.BestMoves}";
			label.HorizontalAlignment = HorizontalAlignment.Center;
			label.VerticalAlignment   = VerticalAlignment.Center;
			label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
 
label.AddThemeColorOverride("font_color", FontColor);

			if (ButtonFont != null)
				label.AddThemeFontOverride("font", ButtonFont);
			label.AddThemeFontSizeOverride("font_size", ButtonFontSize);
 
			btn.AddChild(label);
			btn.Pressed += () =>
			{
				SoundManager.Instance?.PlayClick();
				var gm = GetNode<GameManager>("/root/GameManager");
				gm.LoadLevelByIndex(index);
			};
 
			vbox.AddChild(btn);
		}
	}
}

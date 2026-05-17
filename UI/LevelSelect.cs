using Godot;

public partial class LevelSelect : CanvasLayer
{
	public override void _Ready()
	{
		Layer = 15;

		var backBtn = GetNodeOrNull<TextureButton>("Panel/BackButton");
		if (backBtn != null)
			backBtn.Pressed += () => GetTree().ChangeSceneToFile("res://main.tscn");

		var grid = GetNodeOrNull<GridContainer>("Panel/ScrollContainer/GridContainer");
		if (grid == null) return;

		var gm = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gm?.levels == null) return;

		for (int i = 0; i < gm.levels.Length; i++)
		{
			int index = i;
			var level = gm.levels[i];

			var btn = new TextureButton();
			btn.CustomMinimumSize = new Vector2(160, 80);
			btn.IgnoreTextureSize = true;
			btn.StretchMode = TextureButton.StretchModeEnum.Scale;

			var label = new Label();
			label.Text = $"Уровень {i + 1}";
			label.HorizontalAlignment = HorizontalAlignment.Center;
			label.VerticalAlignment = VerticalAlignment.Center;
			label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			btn.AddChild(label);

			if (!level.isUnlocked)
			{
				btn.Disabled = true;
				label.Text += "\n🔒";
			}

			btn.Pressed += () => gm.LoadLevelByIndex(index);
			grid.AddChild(btn);
		}
	}
}

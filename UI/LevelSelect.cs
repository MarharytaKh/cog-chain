using Godot;
 
public partial class LevelSelect : CanvasLayer
{
	public override void _Ready()
	{
		Layer = 15;
 
		var backBtn = GetNodeOrNull<Button>("Panel/BackButton");
		if (backBtn != null)
			backBtn.Pressed += () => GetTree().ChangeSceneToFile("res://main.tscn");
 
		var grid = GetNodeOrNull<GridContainer>("Panel/ScrollContainer/GridContainer");
		if (grid == null) return;
 
		var gm = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
		if (gm?.levels == null) return;
 
		for (int i = 0; i < gm.levels.Length; i++)
		{
			int index = i;
			var level = gm.levels[i];
 
			var btn = new Button();
			btn.Text = $"Уровень {i + 1}";
			btn.CustomMinimumSize = new Vector2(160, 80);
 
			if (!level.isUnlocked)
			{
				btn.Disabled = true;
				btn.Text += "\n🔒";
			}
 
			btn.Pressed += () =>
			{
				gm.LoadLevelByIndex(index);
			};
 
			grid.AddChild(btn);
		}
	}
}

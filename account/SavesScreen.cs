using Godot;

public partial class SavesScreen : CanvasLayer
{
	public override void _Ready()
	{
		GetNode<Button>("Panel/BackButton").Pressed += () =>
			GetTree().ChangeSceneToFile("res://main.tscn");

		var vbox = GetNode<VBoxContainer>("Panel/ScrollContainer/VBoxContainer");

		if (SaveSystem.CurrentUser == null) return;

		foreach (var lvl in SaveSystem.CurrentUser.LevelResults)
		{
			if (!lvl.Value.Completed) continue;

			int index = lvl.Key;
			var result = lvl.Value;

			var btn = new Button();
			int mins = (int)result.BestTime / 60;
			float secs = result.BestTime % 60;
			btn.Text = $"Уровень {index + 1} — ⏱ {mins}:{secs:00} — 🔧 {result.BestMoves}";
			btn.Pressed += () =>
			{
				var gm = GetNode<GameManager>("/root/GameManager");
				gm.LoadLevelByIndex(index);
			};

			vbox.AddChild(btn);
		}
	}
}

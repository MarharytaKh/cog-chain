using Godot;

public partial class UILayer : Node
{
	public override void _Ready()
	{
		GetNodeOrNull<TextureButton>("UI/Panel/PauseButton")?.Connect("pressed", Callable.From(() =>
		{
			var pause = GD.Load<PackedScene>("res://UI/PauseMenu.tscn").Instantiate();
			GetTree().Root.AddChild(pause);
		}));
	}

	public override void _Process(double delta)
	{
		var gm = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gm == null) return;

		var timeLabel = GetNodeOrNull<Label>("UI/Panel/TimeLabel");
		var movesLabel = GetNodeOrNull<Label>("UI/Panel/MovesLabel");

		if (timeLabel != null)
		{
			float t = gm.GetTime();
			timeLabel.Text = $"Time: {(int)t / 60}:{(t % 60):00}";
		}

		if (movesLabel != null)
			movesLabel.Text = $"Moves: {gm.GetMoves()}";
	}
}

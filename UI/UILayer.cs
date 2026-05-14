using Godot;

public partial class UILayer : Node
{
	public override void _Process(double delta)
	{
		var gm = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gm == null) return;

		var timeLabel = GetNodeOrNull<Label>("UI/Panel/TimeLabel");
		var movesLabel = GetNodeOrNull<Label>("UI/Panel/MovesLabel");

		if (timeLabel != null)
		{
			float t = gm.GetTime();
			timeLabel.Text = $"{(int)t / 60}:{(t % 60):00}";
		}

		if (movesLabel != null)
			movesLabel.Text = $"Ходов: {gm.GetMoves()}";
	}
	public override void _Ready()
{
	GetNode<Button>("UI/Panel/PauseButton").Pressed += () =>
	{
		var pause = GD.Load<PackedScene>("res://UI/PauseMenu.tscn").Instantiate();
		GetTree().Root.AddChild(pause);
	};
}
}

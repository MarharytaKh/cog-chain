using Godot;

public partial class UILayer : Node
{
	public override void _Ready()
	{
		var pauseBtn = GetNodeOrNull<TextureButton>("UI/Panel/PauseButton");
		if (pauseBtn != null)
			pauseBtn.Pressed += () =>
			{
				SoundManager.Instance?.PlayClick();
				var pause = GD.Load<PackedScene>("res://UI/PauseMenu.tscn").Instantiate();
				GetTree().Root.AddChild(pause);
			};
			var pauseLabel = GetNodeOrNull<Label>("UI/Panel/Button2Label");
if (pauseLabel != null) pauseLabel.Text = Tr("PAUSE");
	}

	public override void _Process(double delta)
	{
		var gm = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gm == null) return;

		var timeLabel  = GetNodeOrNull<Label>("UI/Panel/TimeLabel");
		var movesLabel = GetNodeOrNull<Label>("UI/Panel/MovesLabel");

if (timeLabel != null)
{
	float t = gm.GetTime();
	timeLabel.Text = $"{Tr("TIME_LABEL")} {(int)t / 60}:{(t % 60):00}";
}

if (movesLabel != null)
	movesLabel.Text = $"{Tr("MOVES_LABEL")} {gm.GetMoves()}";
	}
}

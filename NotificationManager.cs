using Godot;

public partial class NotificationManager : CanvasLayer
{
	[Export] public float Duration = 2.0f;
	[Export] public float FadeTime = 0.5f;
	[Export] public Godot.Collections.Array<Texture2D> AchievementIcons = new();

	private static readonly string[] AchievementKeys = {
		"first_level", "chain_master", "no_remove", "speed_run",
		"game_complete", "all_gears", "five_stars", "all_stars", "persistent"
	};

	private Label _label;
	private TextureRect _panel;
	private TextureRect _icon;
	private float _timer = 0f;
	private bool _active = false;

	public override void _Ready()
	{
		_label = GetNodeOrNull<Label>("Panel/Label");
		_panel = GetNodeOrNull<TextureRect>("Panel");
		_icon  = GetNodeOrNull<TextureRect>("Panel/Icon");
		if (_panel != null)
			_panel.Modulate = new Color(1, 1, 1, 0);
		Layer = 50;
	}

	public void Show(string message, string achievementKey = "")
	{
		_label.Text = message;
		_timer = 0f;
		_active = true;
		_panel.Visible = true;
		_panel.Modulate = new Color(1, 1, 1, 1);

SoundManager.Instance?.PlayAchievement();
		// Иконка
		if (_icon != null && !string.IsNullOrEmpty(achievementKey))
		{
			int idx = System.Array.IndexOf(AchievementKeys, achievementKey);
			if (idx >= 0 && idx < AchievementIcons.Count)
				_icon.Texture = AchievementIcons[idx];
		}
	}

	public override void _Process(double delta)
	{
		if (!_active) return;
		_timer += (float)delta;
		if (_timer >= Duration)
		{
			float fade = 1f - (_timer - Duration) / FadeTime;
			_panel.Modulate = new Color(1, 1, 1, Mathf.Max(fade, 0f));
			if (fade <= 0f) { _active = false; _panel.Visible = false; }
		}
	}
}

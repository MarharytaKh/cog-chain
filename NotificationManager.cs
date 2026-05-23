using Godot;

public partial class NotificationManager : CanvasLayer
{
	[Export] public float Duration = 2.0f;
	[Export] public float FadeTime = 0.5f;

	private Label _label;
	private TextureRect _panel;
	private float _timer = 0f;
	private bool _active = false;

public override void _Ready()
{
	GD.Print("Notification children: ");
	foreach (Node child in GetChildren())
		GD.Print(" - " + child.Name);
	
	_label = GetNodeOrNull<Label>("Panel/Label");
   _panel = GetNodeOrNull<TextureRect>("Panel");
	GD.Print("Panel found: " + (_panel != null));
	GD.Print("Label found: " + (_label != null));
	if (_panel != null)
		_panel.Modulate = new Color(1, 1, 1, 0);
	Layer = 50;
}

	public void Show(string message)
	{
		_label.Text = message;
		_timer = 0f;
		_active = true;
		_panel.Modulate = new Color(1, 1, 1, 1);
	}

	public override void _Process(double delta)
	{
		if (!_active) return;
		_timer += (float)delta;

		if (_timer >= Duration)
		{
			float fade = 1f - (_timer - Duration) / FadeTime;
			_panel.Modulate = new Color(1, 1, 1, Mathf.Max(fade, 0f));
			if (fade <= 0f) _active = false;
		}
	}
}

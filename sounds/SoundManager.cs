using Godot;

public partial class SoundManager : Node
{
	public static SoundManager Instance;

	[Export] public AudioStream ButtonClick;
	[Export] public AudioStream BackgroundMusic;
	[Export] public AudioStream GearPlace;
	[Export] public AudioStream GearRemove;
	[Export] public AudioStream GearSpin;

	private AudioStreamPlayer _sfxPlayer;
	private AudioStreamPlayer _musicPlayer;
	private AudioStreamPlayer _spinPlayer;

	public override void _Ready()
	{
		Instance = this;

		_sfxPlayer = new AudioStreamPlayer();
		_sfxPlayer.Bus = "SFX";
		AddChild(_sfxPlayer);

		_musicPlayer = new AudioStreamPlayer();
		_musicPlayer.Bus = "Music";
		_musicPlayer.Stream = BackgroundMusic;
		AddChild(_musicPlayer);
		_musicPlayer.Play();

		_spinPlayer = new AudioStreamPlayer();
		_spinPlayer.Bus = "SFX";
		_spinPlayer.Stream = GearSpin;
		AddChild(_spinPlayer);

		SettingsManager.Load();
		SettingsManager.Apply();
	}

	public void PlayClick()
	{
		if (ButtonClick == null) return;
		_sfxPlayer.Stream = ButtonClick;
		_sfxPlayer.Play();
	}

	public void PlaySFX(AudioStream stream)
	{
		if (stream == null) return;
		_sfxPlayer.Stream = stream;
		_sfxPlayer.Play();
	}

	public void PlayGearPlace()  => PlaySFX(GearPlace);
	public void PlayGearRemove() => PlaySFX(GearRemove);

	public void StartSpin()
	{
		if (GearSpin == null) return;
		if (!_spinPlayer.Playing)
			_spinPlayer.Play();
	}
	
	
	public override void _Notification(int what)
{
	if (what == NotificationPredelete)
		StopSpin();
}

	public void StopSpin()
	{
		_spinPlayer.Stop();
	}
	
}

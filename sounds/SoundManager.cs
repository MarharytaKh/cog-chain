using Godot;

public partial class SoundManager : Node
{
	public static SoundManager Instance;
	[Export] public AudioStream ButtonClick;
	[Export] public AudioStream BackgroundMusic;
	[Export] public AudioStream GearPlace;
	[Export] public AudioStream GearRemove;
	[Export] public AudioStream GearSpin;
	[Export] public AudioStream AchievementSound;
	[Export] public AudioStream DoorSound;
	[Export] public AudioStream LampSound;
	[Export] public AudioStream ClockHandSound;
	[Export] public AudioStream KeySound;
	[Export] public AudioStream LiftSound;
	[Export] public AudioStream ArrowSound;
	[Export] public AudioStream RotateSound;

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
		_spinPlayer = new AudioStreamPlayer();
		_spinPlayer.Bus = "SFX";
		_spinPlayer.Stream = GearSpin;
		AddChild(_spinPlayer);
		SettingsManager.Load();
		SettingsManager.Apply();
	}

	public void PlayAchievement()
	{
		if (AchievementSound != null)
		{
			var player = new AudioStreamPlayer();
			player.Stream = AchievementSound;
			player.Bus = "SFX";
			AddChild(player);
			player.Play();
			player.Finished += player.QueueFree;
		}
	}

	public void PlayAnimation(LevelAnimation.AnimationType type)
	{
		AudioStream stream = type switch {
			LevelAnimation.AnimationType.Door      => DoorSound,
			LevelAnimation.AnimationType.Lamp      => LampSound,
			LevelAnimation.AnimationType.ClockHand => ClockHandSound,
			LevelAnimation.AnimationType.Key       => KeySound,
			LevelAnimation.AnimationType.Lift      => LiftSound,
			LevelAnimation.AnimationType.Arrow     => ArrowSound,
			LevelAnimation.AnimationType.Rotate    => RotateSound,
			_ => null
		};
		PlaySFX(stream);
	}

	public void StartMusic()
	{
		if (!_musicPlayer.Playing)
			_musicPlayer.Play();
	}

	public void StopMusic()
	{
		_musicPlayer.Stop();
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

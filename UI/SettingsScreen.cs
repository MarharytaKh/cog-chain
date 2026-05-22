using Godot;

public partial class SettingsScreen : CanvasLayer
{
	private HSlider _musicSlider;
	private HSlider _sfxSlider;
	private TextureButton _languageBtn;
	private Label _languageLabel;

	public override void _Ready()
	{
		Layer = 15;

		_musicSlider   = GetNode<HSlider>("MusicSlider");
		_sfxSlider     = GetNode<HSlider>("SFXSlider");
		_languageBtn   = GetNode<TextureButton>("LanguageButton");
		_languageLabel = GetNode<Label>("LanguageLabel");

		_musicSlider.Value = SettingsManager.MusicVolume;
		_sfxSlider.Value   = SettingsManager.SfxVolume;
		UpdateLanguageLabel();

		_musicSlider.ValueChanged += v =>
		{
			SettingsManager.MusicVolume = (float)v;
			SettingsManager.Apply();
			SettingsManager.Save();
		};

		_sfxSlider.ValueChanged += v =>
		{
			SettingsManager.SfxVolume = (float)v;
			SettingsManager.Apply();
			SettingsManager.Save();
		};

		_languageBtn.Pressed += () =>
		{
			SoundManager.Instance?.PlayClick();
			SettingsManager.ToggleLanguage();
			SettingsManager.Save();
			UpdateLanguageLabel();
		};

		GetNode<TextureButton>("BackButton").Pressed += () =>
		{
			SoundManager.Instance?.PlayClick();
			GetTree().ChangeSceneToFile("res://main.tscn");
		};
	}

	private void UpdateLanguageLabel()
	{
		_languageLabel.Text = SettingsManager.Language == "en" ? "English" : "Polski";
	}
}

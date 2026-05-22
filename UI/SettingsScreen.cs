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
		_languageBtn   = FindChild("LanguageButton") as TextureButton;
		_languageLabel = FindChild("LanguageLabel") as Label;

		_musicSlider.Value = SettingsManager.MusicVolume;
		_sfxSlider.Value   = SettingsManager.SfxVolume;

		RefreshLabels();
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
			RefreshLabels();
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

	private void RefreshLabels()
	{
		var backLabel = GetNodeOrNull<Label>("Button2Label");
		if (backLabel != null) backLabel.Text = Tr("BACK");

		var musicLabel = GetNodeOrNull<Label>("Music");
		if (musicLabel != null) musicLabel.Text = Tr("MUSIC");

		var sfxLabel = GetNodeOrNull<Label>("SFX");
		if (sfxLabel != null) sfxLabel.Text = Tr("SFX");
	}
}

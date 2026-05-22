using Godot;

public static class SettingsManager
{
	public static float MusicVolume = 80f;
	public static float SfxVolume   = 80f;
	public static string Language   = "en";

	private const string SavePath = "user://settings.json";

	public static void Save()
	{
		var data = new Godot.Collections.Dictionary
		{
			["musicVolume"] = MusicVolume,
			["sfxVolume"]   = SfxVolume,
			["language"]    = Language
		};
		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		file.StoreString(Json.Stringify(data));
	}

	public static void Load()
	{
		if (!FileAccess.FileExists(SavePath)) return;
		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		var json = new Json();
		json.Parse(file.GetAsText());
		var data = json.Data.AsGodotDictionary();

		if (data.ContainsKey("musicVolume")) MusicVolume = data["musicVolume"].AsSingle();
		if (data.ContainsKey("sfxVolume"))   SfxVolume   = data["sfxVolume"].AsSingle();
		if (data.ContainsKey("language"))    Language    = data["language"].ToString();
	}

	public static void Apply()
{
	int musicBus = AudioServer.GetBusIndex("Music");
	int sfxBus   = AudioServer.GetBusIndex("SFX");

	if (musicBus >= 0)
		AudioServer.SetBusVolumeDb(musicBus, Mathf.LinearToDb(MusicVolume / 100f));
	if (sfxBus >= 0)
		AudioServer.SetBusVolumeDb(sfxBus, Mathf.LinearToDb(SfxVolume / 100f));
}

	public static void ToggleLanguage()
	{
		Language = Language == "en" ? "pl" : "en";
	}
}

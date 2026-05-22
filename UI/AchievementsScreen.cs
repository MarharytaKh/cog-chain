using Godot;

public partial class AchievementsScreen : CanvasLayer
{
	private static readonly (string key, string name, string desc)[] All = {
		("first_level",   "Первый шаг",    "Пройди первый уровень"),
		("chain_master",  "Мастер цепи",   "Одна шестерёнка запускает 4+ других"),
		("no_remove",     "Чистая работа", "Пройди уровень без удаления шестерёнок"),
		("speed_run",     "Молния",        "Пройди уровень быстрее 15 секунд"),
		("game_complete", "Покоритель",    "Пройди все уровни"),
		("all_gears",     "Без остатка",   "Используй все доступные шестерёнки"),
		("five_stars",    "Перфекционист", "Получи 5 звёзд на любом уровне"),
		("all_stars",     "Абсолют",       "Собери все звёзды на всех уровнях"),
		("persistent",    "Настойчивость", "Перезапусти уровень 3 раза подряд"),
	};

	public override void _Ready()
	{
		Layer = 15;

		var backBtn = GetNodeOrNull<TextureButton>("BackButton");
		if (backBtn != null)
			backBtn.Pressed += () =>
			{
				SoundManager.Instance?.PlayClick();
				GetTree().ChangeSceneToFile("res://main.tscn");
			};

		var vbox = GetNodeOrNull<VBoxContainer>("ScrollContainer/VBoxContainer");
		if (vbox == null) return;

		foreach (var (key, name, desc) in All)
		{
			bool unlocked = SaveSystem.HasAchievement(key);
			var label = new Label();
			label.Text = unlocked ? $"✅ {name} — {desc}" : $"🔒 ???";
			label.Modulate = unlocked ? new Color(1, 1, 1) : new Color(0.5f, 0.5f, 0.5f);
			vbox.AddChild(label);
		}
	}
}

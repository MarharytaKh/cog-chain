using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class RankingScene : CanvasLayer
{
	[Export] public FontFile ButtonFont;
	[Export] public int FontSize = 24;
	[Export] public Color FontColor = new Color(1, 1, 1);
	[Export] public Texture2D RowTexture;
	[Export] public Vector2 RowSize = new Vector2(0, 70);

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

		var titleLabel = GetNodeOrNull<Label>("TitleLabel");
		if (titleLabel != null) titleLabel.Text = Tr("RANKING");

		var vbox = GetNodeOrNull<VBoxContainer>("ScrollContainer/VBoxContainer");
		if (vbox == null) return;

		var loadingLabel = new Label();
		loadingLabel.Text = Tr("LOADING");
		loadingLabel.HorizontalAlignment = HorizontalAlignment.Center;
		vbox.AddChild(loadingLabel);

		_ = LoadRanking(vbox, loadingLabel);
	}

	private async System.Threading.Tasks.Task LoadRanking(VBoxContainer vbox, Label loadingLabel)
	{
		try
		{
			var json = await FirebaseManager.GetRankings();
			loadingLabel.QueueFree();

			if (json == null)
			{
				var empty = new Label();
				empty.Text = Tr("NO_RANKING_DATA");
				empty.HorizontalAlignment = HorizontalAlignment.Center;
				vbox.AddChild(empty);
				return;
			}

			// Парсим и сортируем
			var entries = new List<(string username, int stars)>();
			using var doc = JsonDocument.Parse(json);
			foreach (var item in doc.RootElement.EnumerateObject())
			{
				string username = "";
				int totalStars = 0;
				if (item.Value.TryGetProperty("username", out var u))
					username = u.GetString() ?? "";
				if (item.Value.TryGetProperty("totalStars", out var s))
					totalStars = s.GetInt32();
				entries.Add((username, totalStars));
			}

			entries.Sort((a, b) => b.stars.CompareTo(a.stars));

			// Отображаем
			for (int i = 0; i < entries.Count; i++)
			{
				var (username, stars) = entries[i];
				bool isCurrentUser = username == SaveSystem.CurrentUser?.Username;

				var row = new TextureButton();
				row.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
				row.IgnoreTextureSize = true;
				row.StretchMode = TextureButton.StretchModeEnum.Scale;
				row.CustomMinimumSize = RowSize;
				if (RowTexture != null) row.TextureNormal = RowTexture;

				var label = new Label();
				label.Text = $"#{i + 1}   {username}   ★ {stars}";
				label.HorizontalAlignment = HorizontalAlignment.Center;
				label.VerticalAlignment = VerticalAlignment.Center;
				label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
				if (ButtonFont != null) label.AddThemeFontOverride("font", ButtonFont);
				label.AddThemeFontSizeOverride("font_size", FontSize);

				// Подсвечиваем текущего игрока
				var color = isCurrentUser ? new Color(1, 0.85f, 0.2f) : FontColor;
				label.AddThemeColorOverride("font_color", color);

				row.AddChild(label);
				vbox.AddChild(row);
			}
		}
		catch (Exception e)
		{
			GD.PrintErr("RankingScreen error: " + e.Message);
		}
	}
}

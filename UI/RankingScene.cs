using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class RankingScene : CanvasLayer
{
	[Export] public FontFile ButtonFont;
	[Export] public int FontSize = 24;
	[Export] public Color FontColor = new Color(1f, 1f, 1f);
	[Export] public Color CurrentUserColor = new Color(1f, 0.85f, 0.2f);
	[Export] public Color GoldColor = new Color(1f, 0.84f, 0f);
	[Export] public Color SilverColor = new Color(0.75f, 0.75f, 0.75f);
	[Export] public Color BronzeColor = new Color(0.8f, 0.5f, 0.2f);
	[Export] public Texture2D RowTexture;
	[Export] public Texture2D TopRowTexture;
	[Export] public Vector2 RowSize = new Vector2(0, 70);
	[Export] public int MaxEntries = 50;

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

			for (int i = 0; i < Mathf.Min(entries.Count, MaxEntries); i++)
			{
				var (username, stars) = entries[i];
				bool isCurrentUser = username == SaveSystem.CurrentUser?.Username;
				bool isTop3 = i < 3;

				var row = new TextureButton();
				row.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
				row.IgnoreTextureSize = true;
				row.StretchMode = TextureButton.StretchModeEnum.Scale;
				row.CustomMinimumSize = RowSize;

				if (isTop3 && TopRowTexture != null)
					row.TextureNormal = TopRowTexture;
				else if (RowTexture != null)
					row.TextureNormal = RowTexture;

				string medal = i switch {
					0 => "🥇 ",
					1 => "🥈 ",
					2 => "🥉 ",
					_ => $"#{i + 1}  "
				};

				var label = new Label();
				label.Text = $"{medal} {username}   ★ {stars}";
				label.HorizontalAlignment = HorizontalAlignment.Center;
				label.VerticalAlignment = VerticalAlignment.Center;
				label.SetAnchorsPreset(Control.LayoutPreset.FullRect);

				if (ButtonFont != null) label.AddThemeFontOverride("font", ButtonFont);
				label.AddThemeFontSizeOverride("font_size", FontSize);

				Color color;
				if (isCurrentUser)      color = CurrentUserColor;
				else if (i == 0)        color = GoldColor;
				else if (i == 1)        color = SilverColor;
				else if (i == 2)        color = BronzeColor;
				else                    color = FontColor;

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

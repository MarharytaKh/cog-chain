using Godot;
public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<TextureButton>("Button").Pressed += () =>
			GetTree().ChangeSceneToFile("res://UI/LevelSelect.tscn");

		GetNode<TextureButton>("LoadButton").Pressed += () =>
			GetTree().ChangeSceneToFile("res://account/SavesScreen.tscn");

		GetNode<TextureButton>("RankingButton2").Pressed += () =>
		{
			SoundManager.Instance?.PlayClick();
			GetTree().ChangeSceneToFile("res://UI/RankingScene.tscn");
		};

		var rankLabel = GetNodeOrNull<Label>("RankingButton2/LoadButtonRanking");
		if (rankLabel != null) rankLabel.Text = Tr("RANKING");
	}
}

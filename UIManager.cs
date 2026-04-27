using Godot;
using System.Collections.Generic;
public partial class UIManager : CanvasLayer
{
	// Instance убираем
	private Node currentScreen;
	private Dictionary<string, PackedScene> screens = new Dictionary<string, PackedScene>();

	public override void _Ready()
	{
		Register("hud",            "res://UI/UILayer.tscn");
		Register("level_complete", "res://UI/LevelComplete.tscn");
		Register("pause",          "res://UI/PauseMenu.tscn");
		Register("main_menu",      "res://UI/MainMenu.tscn");
	}
	// остальное без изменений

	private void Register(string key, string path)
	{
		if (ResourceLoader.Exists(path))
			screens[key] = GD.Load<PackedScene>(path);
		else
			GD.PrintErr($"UIManager: не найден экран {path}");
	}

  public void Show(string key)
  {
	  currentScreen?.QueueFree();
	  currentScreen = null;

	  if (!screens.ContainsKey(key))
	  {
		GD.PrintErr($"UIManager: экран '{key}' не зарегистрирован");
		return;
	  }

	  currentScreen = screens[key].Instantiate();
	  AddChild(currentScreen);
	
	  // поднимаем на верх
	  Layer = 10;
	  GD.Print($"UIManager показывает: {key}, Layer={Layer}");
	}

	public void Hide()
	{
		currentScreen?.QueueFree();
		currentScreen = null;
	}

	public Node GetCurrentScreen() => currentScreen;
}

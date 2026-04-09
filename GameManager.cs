using Godot;
using System.Collections.Generic;
public partial class GameManager : Node
{
	public static Axis SelectedAxis;
	private Level currentLevel = new Level();

	public override void _Ready()
	{
		currentLevel.GearSource = GetTree().CurrentScene.GetNode<Motor>("Motor");
		currentLevel.GearTarget = GetTree().CurrentScene.GetNode<Node3D>("Target");
		currentLevel.maxGears = 5;
		
		// подключаем сигнал кнопки прямо из кода — надёжнее чем через редактор
		var button = GetTree().CurrentScene.GetNode<Button>("UI/Panel/Button");
		button.Pressed += _on_button_pressed;
	}

	// метод переименован — подключается через код выше
private void _on_button_pressed()
{
	if (SelectedAxis == null || SelectedAxis.HasGear)
		return;

	Vector3 targetPos = SelectedAxis.GlobalPosition;
	var motor = currentLevel.GearSource;

	var gearScene = GD.Load<PackedScene>("res://gear.tscn");
	var gear = gearScene.Instantiate<Gear>();

	// --- проверки через targetPos (БЕЗ GlobalPosition у gear) ---

	foreach (var g in GetAllGears())
	{
		if (gear.OverlapsAtPos(targetPos, g))
		{
			GD.Print("Overlap!");
			return;
		}
	}

	if (gear.OverlapsMotorAtPos(targetPos, motor))
	{
		GD.Print("Overlap с мотором!");
		return;
	}

	bool canMesh = false;

	if (gear.CanMeshMotorAtPos(targetPos, motor))
		canMesh = true;

	foreach (var g in GetAllGears())
	{
		if (gear.CanMeshAtPos(targetPos, g))
		{
			canMesh = true;
			break;
		}
	}

	// --- теперь можно добавить ---
	AddChild(gear);
	gear.GlobalPosition = targetPos;
// фазовый снап — ОДИН РАЗ при установке

// фазовый снап ТОЛЬКО если реально есть контакт

if (gear.CanMeshMotorAtPos(targetPos, motor))
{
	gear.SnapPhaseWithMotor(motor);
}
else
{
	foreach (var g in GetAllGears())
	{
		if (gear.CanMeshAtPos(targetPos, g))
		{
			gear.SnapPhaseWithGear(g);
			break;
		}
	}
}

	SelectedAxis.HasGear = true;

	Recalculate();
}

	private void Recalculate()
{
	var gears = GetAllGears();
	var motor = currentLevel.GearSource;

	PhysicsEngine.BuildGraph(motor, gears);
}

	private List<Gear> GetAllGears()
	{
		var gears = new List<Gear>();
		foreach (Node node in GetChildren())
		{
			if (node is Gear g)
				gears.Add(g);
		}
		return gears;
	}
}

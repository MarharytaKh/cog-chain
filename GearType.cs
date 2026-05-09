using Godot;

[GlobalClass]
public partial class GearType : Resource
{
	[Export] public string gearName = "Standard";
	[Export] public float Radius = 1.23f;

	[Export] public int ToothCount = 20;
	[Export] public float SpeedMultiplier = 1.0f;
	[Export] public float AngleOffset = 0f;

	[Export] public PackedScene scenePrefab;
	
	
	[Export] public string[] compatibleWith;
	[Export] public bool InvertDirection = false;
}

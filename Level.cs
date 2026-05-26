using Godot;

[GlobalClass]
public partial class Level : Resource
{
	[Export] public int LevelID = 1;
	[Export] public int MaxGears = 5;
	[Export] public int Difficulty = 1;
	[Export] public int starsRequired = 1;
	[Export] public bool isUnlocked = true;
	[Export] public PackedScene levelScene;
	[Export] public GearType[] availableGearTypes;
	[Export] public int[] availableGearCounts;
	[Export] public float[] StarTimeLimits = { 150f, 90f, 60f, 45f, 30f };
	[Export] public int[] StarMoveLimits = { 35, 20, 15, 10, 5 };

	public void Unlock()
	{
		isUnlocked = true;
	}
}

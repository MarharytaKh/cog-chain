using Godot;
using System.Collections.Generic;

public class Level
{
	public int levelID;
	public int maxGears;
	public int difficulty;

	public Motor GearSource;
	public Node3D GearTarget;

	public bool isUnlocked = true;

	public bool Validate(List<Gear> gears)
	{
		return false;
	}
}

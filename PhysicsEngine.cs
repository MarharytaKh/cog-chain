using System.Collections.Generic;
using Godot;
 
public static class PhysicsEngine
{
	private static bool AreCompatible(GearType a, GearType b)
{
	if (a == null || b == null) return true;
	if (a.compatibleWith == null || a.compatibleWith.Length == 0) return true;
	if (b == null) return true;
	foreach (var name in a.compatibleWith)
		if (name == b.gearName) return true;
	return false;
}
 
	public static void BuildGraph(Motor motor, List<Gear> gears, Target target)
	{
		foreach (var g in gears)
			g.Reset();
 
		motor.Children.Clear();
		target.ParentGear = null;
 
		HashSet<Gear> visited = new HashSet<Gear>();
		Queue<Gear> queue = new Queue<Gear>();
 
		foreach (var g in gears)
		{
			float dist = g.GlobalPosition.DistanceTo(motor.GlobalPosition);
			if (Mathf.Abs(dist - (g.Radius + motor.Radius)) < 0.2f)
			{
				if (g.config != null && g.config.gearName == "Big") continue;
				g.MotorParent = motor;
				motor.Children.Add(g);
				visited.Add(g);
				queue.Enqueue(g);
			}
		}
 
		while (queue.Count > 0)
		{
			var current = queue.Dequeue();
 
			float distToTarget = current.GlobalPosition.DistanceTo(target.GlobalPosition);
			if (Mathf.Abs(distToTarget - (current.Radius + target.Radius)) < 0.2f)
			{
				bool canConnectTarget = current.config == null || current.config.gearName != "Big";
				if (canConnectTarget)
				{
					target.ParentGear = current;
					target.Activate();
				}
			}
 
			foreach (var other in gears)
			{
				if (other == current) continue;
				if (visited.Contains(other)) continue;
 
				float dist = current.GlobalPosition.DistanceTo(other.GlobalPosition);
				if (Mathf.Abs(dist - (current.Radius + other.Radius)) < 0.2f)
				{
					if (AreCompatible(current.config, other.config) && AreCompatible(other.config, current.config))
					{
						other.SetParent(current);
						visited.Add(other);
						queue.Enqueue(other);
					}
				}
			}
		}
	}
}

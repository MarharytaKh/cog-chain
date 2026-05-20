using System.Collections.Generic;
using Godot;

public static class PhysicsEngine
{
	private static bool AreCompatible(GearType a, GearType b)
	{
		if (a == null || b == null) return true;
		if (a.compatibleWith == null || a.compatibleWith.Length == 0) return true;
		foreach (var name in a.compatibleWith)
			if (name == b.gearName) return true;
		return false;
	}

	private static bool CanConnect(float dist, float r1, float r2, Basis b1, Basis b2)
	{
		float expected = r1 + r2;
		float dot = Mathf.Abs(b1.Column1.Normalized().Dot(b2.Column1.Normalized()));
 
		if (dot < 0.05f)
		{
			if (dist < expected * 0.3f) return false;//////////////////////////
			if (dist > expected * 0.6f) return false;
			return true;
		}
		else
		{
			if (dot < 0.7f) return false;
			if (dist < expected * 0.6f) return false;
			return Mathf.Abs(dist - expected) < 0.2f;
		}
	}

	public static void BuildGraph(Motor motor, List<Gear> gears, List<Target> targets, GameManager gm = null)
	{
		foreach (var g in gears) g.Reset();
		motor.Children.Clear();
		foreach (var t in targets)
		{
			t.ParentGear = null;
			t.Children.Clear();
			t.Activated = false;
		}

		HashSet<Gear> visited = new HashSet<Gear>();
		Queue<Gear> queue = new Queue<Gear>();

		// Шестерёнка + мотор
		foreach (var g in gears)
		{
			float dist = g.GlobalPosition.DistanceTo(motor.GlobalPosition);
			if (!CanConnect(dist, g.Radius, motor.Radius, g.initialBasis, motor.initialBasis)) continue;
			if (g.config != null && g.config.gearName == "Big") continue;
			g.MotorParent = motor;
			motor.Children.Add(g);
			visited.Add(g);
			queue.Enqueue(g);
		}

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();

			// Шестерёнка + таргет
			foreach (var target in targets)
			{
				float dist = current.GlobalPosition.DistanceTo(target.GlobalPosition);
				if (!CanConnect(dist, current.Radius, target.Radius, current.initialBasis, target._initialBasis)) continue;
				if (current.config == null || current.config.gearName != "Big")
				{
					// Таргет ещё не получал родителя — назначаем
					if (target.ParentGear == null)
					{
						target.ParentGear = current;
					}
// После target.Activate() в цикле adj:
GD.Print($"Ищу шестерёнки рядом с таргетом...");
foreach (var adj in gears)
{
	if (visited.Contains(adj)) continue;
	float adjDist = adj.GlobalPosition.DistanceTo(target.GlobalPosition);
	float adjExp = adj.Radius + target.Radius;
	GD.Print($"  adj dist={adjDist:F2} exp={adjExp:F2} can={CanConnect(adjDist, adj.Radius, target.Radius, adj.initialBasis, target._initialBasis)}");
	if (CanConnect(adjDist, adj.Radius, target.Radius, adj.initialBasis, target._initialBasis))
	{
		adj.TargetParent = target;
		target.Children.Add(adj);
		visited.Add(adj);
		queue.Enqueue(adj);
		GD.Print($"  → подключена к таргету!");
	}
}
				}
			}

			// Шестерёнка + шестерёнка
			foreach (var other in gears)
			{
				if (other == current) continue;
				if (visited.Contains(other)) continue;
				float dist = current.GlobalPosition.DistanceTo(other.GlobalPosition);
				if (!CanConnect(dist, current.Radius, other.Radius, current.initialBasis, other.initialBasis)) continue;
				if (AreCompatible(current.config, other.config) && AreCompatible(other.config, current.config))
				{
					other.SetParent(current);
					visited.Add(other);
					queue.Enqueue(other);
				}
				else
				{
					gm?.ShowNotification("Шестерёнки несовместимы!");
				}
			}
		}
	}
}

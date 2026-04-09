using System.Collections.Generic;
using Godot;

public static class PhysicsEngine
{
	public static void BuildGraph(Motor motor, List<Gear> gears)
	{
		foreach (var g in gears)
			g.Reset();

		motor.Children.Clear();

		HashSet<Gear> visited = new HashSet<Gear>();
		Queue<Gear> queue = new Queue<Gear>();

		// старт от мотора
		foreach (var g in gears)
		{
			float dist = g.GlobalPosition.DistanceTo(motor.GlobalPosition);
			if (Mathf.Abs(dist - (g.radius + motor.Radius)) < 0.5f)
			{
				g.MotorParent = motor;
				motor.Children.Add(g);

				visited.Add(g);
				queue.Enqueue(g);
			}
		}

		// BFS без зацикливания
		while (queue.Count > 0)
		{
			var current = queue.Dequeue();

			foreach (var other in gears)
			{
				if (other == current) continue;
				if (visited.Contains(other)) continue;

				float dist = current.GlobalPosition.DistanceTo(other.GlobalPosition);
				if (Mathf.Abs(dist - (current.radius + other.radius)) < 0.5f)
				{
					other.SetParent(current);
					visited.Add(other);
					queue.Enqueue(other);
				}
			}
		}
	}
}

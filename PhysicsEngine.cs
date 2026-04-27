using System.Collections.Generic;
using Godot;

/// <summary>
/// Statyczna klasa budująca graf przekładni metodą BFS.
/// Bezstanowa — wszystkie dane przekazywane są przez parametry metod.
/// </summary>
/// <remarks>
/// Złożoność: O(N²) względem liczby kół. Tolerancja zazębienia: 0.5 jednostki sceny.
/// </remarks>
public static class PhysicsEngine
{
	/// <summary>
	/// Buduje graf przekładni metodą BFS zaczynając od silnika i wykrywa,
	/// czy łańcuch dotarł do węzła <paramref name="target"/>.
	/// </summary>
	/// <remarks>
	/// Przed budowaniem resetuje stan wszystkich kół (<see cref="Gear.Reset"/>),
	/// czyści <see cref="Motor.Children"/> i zeruje <c>target.ParentGear</c>.
	/// Graf jest acyklicznym drzewem zakorzenionym w silniku.
	/// </remarks>
	/// <param name="motor">Korzeń grafu — punkt startowy BFS.</param>
	/// <param name="gears">Lista wszystkich kół na poziomie.</param>
	/// <param name="target">Węzeł docelowy; aktywowany po wykryciu połączenia.</param>
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
				target.ParentGear = current;
				target.Activate();
			}

			foreach (var other in gears)
			{
				if (other == current) continue;
				if (visited.Contains(other)) continue;

				float dist = current.GlobalPosition.DistanceTo(other.GlobalPosition);
				if (Mathf.Abs(dist - (current.Radius + other.Radius)) < 0.2f)
				{
					other.SetParent(current);
					visited.Add(other);
					queue.Enqueue(other);
				}
			}
		}
	}
}

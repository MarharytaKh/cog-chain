using System.Collections.Generic;
using Godot;

/// <summary>
/// Statyczny silnik budowania grafu przekładni.
/// Określa topologię mechanizmu (kto od kogo odbiera obrót)
/// i sprawdza, czy łańcuch kół zębatych dotarł do węzła docelowego.
/// Nie przechowuje stanu — wszystkie dane są przekazywane przy każdym wywołaniu.
/// </summary>
public static class PhysicsEngine
{
	/// <summary>
	/// Buduje graf przekładni metodą przeszukiwania wszerz (BFS) zaczynając od silnika.
	/// <para>
	/// Algorytm:
	/// <list type="number">
	///   <item>Resetuje stan wszystkich kół zębatych i celu.</item>
	///   <item>Znajduje koła bezpośrednio zazębione z silnikiem i dodaje je do kolejki.</item>
	///   <item>Dla każdego koła z kolejki szuka sąsiadów i sprawdza zazębienie z celem.</item>
	///   <item>Jeśli cel jest zazębiony — aktywuje go.</item>
	/// </list>
	/// </para>
	/// Tolerancja zazębienia: odległość między środkami ±0.5 od sumy promieni.
	/// </summary>
	/// <param name="motor">Silnik — korzeń grafu przekładni.</param>
	/// <param name="gears">Lista wszystkich kół zębatych umieszczonych na poziomie.</param>
	/// <param name="target">Węzeł docelowy, który należy osiągnąć łańcuchem przekładni.</param>
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
			if (Mathf.Abs(dist - (g.Radius + motor.Radius)) < 0.5f)
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
			if (Mathf.Abs(distToTarget - (current.Radius + target.Radius)) < 0.5f)
			{
				target.ParentGear = current;
				target.Activate();
			}

			foreach (var other in gears)
			{
				if (other == current) continue;
				if (visited.Contains(other)) continue;

				float dist = current.GlobalPosition.DistanceTo(other.GlobalPosition);
				if (Mathf.Abs(dist - (current.Radius + other.Radius)) < 0.5f)
				{
					other.SetParent(current);
					visited.Add(other);
					queue.Enqueue(other);
				}
			}
		}
	}
}

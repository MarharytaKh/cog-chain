using Godot;
using System.Collections.Generic;

/// <summary>
/// Silnik — źródło obrotu w mechanizmie.
/// Obraca się z zadaną prędkością kątową i przekazuje ruch
/// wszystkim bezpośrednio zazębionym kołom zębatym przez listę <see cref="Children"/>.
/// </summary>
public partial class Motor : Node3D
{
	/// <summary>
	/// Prędkość kątowa obrotu silnika (rad/s).
	/// Ustawiana przez inspektor Godot.
	/// </summary>
	[Export] public float Speed = 1.0f;

	/// <summary>
	/// Promień wieńca zębatego silnika w jednostkach sceny.
	/// Używany przy obliczaniu zazębienia z sąsiednimi kołami.
	/// </summary>
	[Export] public float Radius = 1.23f;

	/// <summary>
	/// Liczba zębów silnika.
	/// Określa przełożenie przy obliczaniu prędkości kątowej podrzędnych kół.
	/// </summary>
	[Export] public int ToothCount = 20;

	/// <summary>
	/// Bieżący kąt obrotu silnika w radianach, akumulowany każdą klatkę.
	/// Normalizowany do zakresu [0, 2π].
	/// </summary>
	public float angle = 0f;

	/// <summary>
	/// Lista kół zębatych bezpośrednio zazębionych z silnikiem.
	/// Wypełniana przez <see cref="PhysicsEngine.BuildGraph"/>.
	/// </summary>
	public List<Gear> Children = new List<Gear>();

	/// <summary>
	/// Aktualizuje kąt obrotu silnika i rekurencyjnie wywołuje aktualizację
	/// wszystkich podrzędnych kół zębatych.
	/// </summary>
	/// <param name="delta">Czas od poprzedniej klatki (sekundy).</param>
	public override void _Process(double delta)
	{
		angle += (float)delta * Speed;
		angle = angle % (2f * Mathf.Pi);
		Rotation = new Vector3(0, angle, 0);
		foreach (var g in Children)
			g.UpdateRotation();
	}
}

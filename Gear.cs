using Godot;
using System.Collections.Generic;

/// <summary>
/// Koło zębate — główny obiekt gry umieszczany przez gracza na osiach.
/// Odbiera obrót od silnika lub nadrzędnego koła zębatego i przekazuje go do podrzędnych.
/// Przechowuje pozycję w grafie przekładni, bieżący kąt i przesunięcie fazy.
/// </summary>
public partial class Gear : Node3D
{
	/// <summary>
	/// Konfiguracja koła zębatego (typ, promień, zęby, przesunięcie kąta).
	/// Ustawiana przez inspektor Godot lub podczas tworzenia instancji.
	/// </summary>
	[Export] public GearType config;

	/// <summary>
	/// Promień wieńca zębatego w jednostkach sceny.
	/// Inicjalizowany z <see cref="config"/> w metodzie <see cref="_Ready"/>.
	/// </summary>
	[Export] public float Radius = 1.23f;

	/// <summary>
	/// Liczba zębów koła zębatego.
	/// Używana do obliczenia przełożenia.
	/// </summary>
	[Export] public int ToothCount = 20;

	/// <summary>
	/// Nadrzędne koło zębate w grafie przekładni.
	/// <c>null</c>, jeśli koło jest połączone bezpośrednio z silnikiem.
	/// </summary>
	public Gear Parent;

	/// <summary>
	/// Referencja do silnika przy bezpośrednim zazębieniu.
	/// Wzajemnie wyklucza się z <see cref="Parent"/>.
	/// </summary>
	public Motor MotorParent;

	/// <summary>
	/// Lista podrzędnych kół zębatych odbierających obrót od tego koła.
	/// </summary>
	public List<Gear> Children = new List<Gear>();

	/// <summary>
	/// Bieżący kąt obrotu w radianach.
	/// </summary>
	public float angle = 0f;

	/// <summary>
	/// Przesunięcie fazy zapewniające prawidłowe zazębienie zębów.
	/// Obliczane podczas przyciągania do silnika lub innego koła.
	/// </summary>
	public float phaseOffset = 0f;

	/// <summary>
	/// Inicjalizuje parametry z <see cref="config"/>, jeśli konfiguracja jest ustawiona.
	/// </summary>
	public override void _Ready()
	{
		if (config != null)
		{
			Radius = config.Radius;
			ToothCount = config.ToothCount;
		}
	}

	/// <summary>
	/// Resetuje stan koła zębatego: czyści powiązania w grafie przekładni i przesunięcie fazy.
	/// Wywoływana na początku każdego przeliczenia grafu.
	/// </summary>
	public void Reset()
	{
		Parent = null;
		MotorParent = null;
		Children.Clear();
		phaseOffset = 0f;
	}

	/// <summary>
	/// Sprawdza, czy koło zębate może zazębić się z innym na podanej pozycji.
	/// Kryterium: odległość między środkami równa sumie promieni (tolerancja 0.2).
	/// </summary>
	/// <param name="pos">Sprawdzana pozycja tego koła.</param>
	/// <param name="other">Inne koło zębate do sprawdzenia zazębienia.</param>
	/// <returns><c>true</c>, jeśli zazębienie jest możliwe.</returns>
	public bool CanMeshAtPos(Vector3 pos, Gear other)
	{
		float dist = pos.DistanceTo(other.GlobalPosition);
		return Mathf.Abs(dist - (Radius + other.Radius)) < 0.2f;
	}

	/// <summary>
	/// Sprawdza, czy koło zębate może zazębić się z silnikiem na podanej pozycji.
	/// </summary>
	/// <param name="pos">Sprawdzana pozycja tego koła.</param>
	/// <param name="motor">Silnik do sprawdzenia zazębienia.</param>
	/// <returns><c>true</c>, jeśli zazębienie z silnikiem jest możliwe.</returns>
	public bool CanMeshMotorAtPos(Vector3 pos, Motor motor)
	{
		float dist = pos.DistanceTo(motor.GlobalPosition);
		return Mathf.Abs(dist - (Radius + motor.Radius)) < 0.2f;
	}

	/// <summary>
	/// Sprawdza, czy koło zębate pokrywa się z innym na podanej pozycji (kolizja).
	/// </summary>
	/// <param name="pos">Sprawdzana pozycja tego koła.</param>
	/// <param name="other">Inne koło zębate.</param>
	/// <returns><c>true</c>, jeśli koła się przecinają.</returns>
	public bool OverlapsAtPos(Vector3 pos, Gear other)
	{
		float dist = pos.DistanceTo(other.GlobalPosition);
		return dist < (Radius + other.Radius) - 0.1f;
	}

	/// <summary>
	/// Sprawdza, czy koło zębate pokrywa się z silnikiem na podanej pozycji (kolizja).
	/// </summary>
	/// <param name="pos">Sprawdzana pozycja tego koła.</param>
	/// <param name="motor">Silnik.</param>
	/// <returns><c>true</c>, jeśli koło przecina się z silnikiem.</returns>
	public bool OverlapsMotorAtPos(Vector3 pos, Motor motor)
	{
		float dist = pos.DistanceTo(motor.GlobalPosition);
		return dist < (Radius + motor.Radius) - 0.1f;
	}

	/// <summary>
	/// Ustawia nadrzędne koło zębate i dodaje bieżące do jego listy podrzędnych.
	/// </summary>
	/// <param name="parent">Nadrzędne koło zębate w grafie przekładni.</param>
	public void SetParent(Gear parent)
	{
		Parent = parent;
		parent.Children.Add(this);
	}

	/// <summary>
	/// Przelicza kąt obrotu na podstawie przełożenia do rodzica
	/// i rekurencyjnie aktualizuje wszystkie podrzędne koła.
	/// Wywoływana każdą klatkę przez <see cref="Motor._Process"/>.
	/// </summary>
	public void UpdateRotation()
	{
		if (MotorParent != null)
		{
			float ratio = (float)MotorParent.ToothCount / (float)ToothCount;
			angle = (-MotorParent.angle * ratio) + phaseOffset;
		}
		else if (Parent != null)
		{
			float ratio = (float)Parent.ToothCount / (float)ToothCount;
			angle = (-Parent.angle * ratio) + phaseOffset;
		}
		else return;
		Rotation = new Vector3(0, angle, 0);
		foreach (var c in Children)
			c.UpdateRotation();
	}

	/// <summary>
	/// Oblicza i ustawia przesunięcie fazy dla prawidłowego zazębienia zębów z silnikiem.
	/// Najbliższa szczelina zębów silnika w punkcie styku jest używana jako punkt odniesienia.
	/// Uwzględnia <see cref="GearType.AngleOffset"/> z konfiguracji.
	/// </summary>
	/// <param name="motor">Silnik, z którym synchronizowana jest faza.</param>
	public void SnapPhaseWithMotor(Motor motor)
	{
		Vector3 dir = (GlobalPosition - motor.GlobalPosition).Normalized();
		float contactAngle = Mathf.Atan2(dir.X, dir.Z);
		float motorTooth = (2f * Mathf.Pi) / motor.ToothCount;
		float motorPhaseAtContact = motor.angle + contactAngle;
		float motorToothIndex = motorPhaseAtContact / motorTooth;
		float nearestGap = (Mathf.Floor(motorToothIndex) + 0.5f) * motorTooth;
		float desiredAngle = -(nearestGap - contactAngle) * (motor.Radius / Radius);
		phaseOffset = desiredAngle - (-motor.angle * (motor.Radius / Radius));
		angle = desiredAngle;
		Rotation = new Vector3(0, angle, 0);

		angle += config != null ? config.AngleOffset : 0f;
		phaseOffset += config != null ? config.AngleOffset : 0f;
		Rotation = new Vector3(0, angle, 0);
	}

	/// <summary>
	/// Oblicza i ustawia przesunięcie fazy dla prawidłowego zazębienia z innym kołem zębatym.
	/// </summary>
	/// <param name="other">Koło zębate, z którym synchronizowana jest faza.</param>
	public void SnapPhaseWithGear(Gear other)
	{
		Vector3 dir = (GlobalPosition - other.GlobalPosition).Normalized();
		float contactAngle = Mathf.Atan2(dir.X, dir.Z);
		float otherTooth = (2f * Mathf.Pi) / other.ToothCount;
		float otherPhaseAtContact = other.angle + contactAngle;
		float otherToothIndex = otherPhaseAtContact / otherTooth;
		float nearestGap = (Mathf.Floor(otherToothIndex) + 0.5f) * otherTooth;
		float desiredAngle = -(nearestGap - contactAngle) * (other.Radius / Radius);
		phaseOffset = desiredAngle - (-other.angle * (other.Radius / Radius));
		angle = desiredAngle;
		Rotation = new Vector3(0, angle, 0);
	}
}

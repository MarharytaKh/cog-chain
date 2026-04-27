using Godot;
using System.Collections.Generic;

/// <summary>
/// Koło zębate — główny obiekt gry umieszczany przez gracza na osiach.
/// Przechowuje pozycję w grafie przekładni, oblicza własny kąt obrotu
/// i propaguje ruch do potomnych kół.
/// </summary>
public partial class Gear : Node3D
{
	/// <summary>
	/// Konfiguracja koła (typ, promień, liczba zębów, korekcja kąta).
	/// Ustawiana przez <see cref="GameManager._on_button_pressed"/> przed dodaniem do sceny.
	/// </summary>
	[Export] public GearType config;

	/// <summary>
	/// Promień wieńca zębatego w jednostkach sceny.
	/// Inicjalizowany z <see cref="GearType.Radius"/> w <see cref="_Ready"/>.
	/// Używany przez <see cref="PhysicsEngine.BuildGraph"/> i <see cref="UpdateRotation"/>.
	/// </summary>
	[Export] public float Radius = 1.23f;

	/// <summary>
	/// Liczba zębów koła. Inicjalizowana z <see cref="GearType.ToothCount"/> w <see cref="_Ready"/>.
	/// Wyznacza przełożenie: <c>ratio = parentToothCount / thisToothCount</c>.
	/// </summary>
	[Export] public int ToothCount = 20;

	/// <summary>
	/// Nadrzędne koło w grafie. Wzajemnie wyklucza się z <see cref="MotorParent"/>.
	/// Ustawiane przez <see cref="SetParent"/>; zerowane przez <see cref="Reset"/>.
	/// </summary>
	public Gear Parent;

	/// <summary>
	/// Silnik jako rodzic — ustawiany dla kół bezpośrednio zazębionych z silnikiem.
	/// Wzajemnie wyklucza się z <see cref="Parent"/>.
	/// </summary>
	public Motor MotorParent;

	/// <summary>
	/// Lista kół potomnych otrzymujących obrót od tego koła.
	/// Wypełniana przez <see cref="PhysicsEngine.BuildGraph"/>; czyszczona przez <see cref="Reset"/>.
	/// </summary>
	public List<Gear> Children = new List<Gear>();

	/// <summary>Bieżący kąt obrotu koła w radianach, obliczany w <see cref="UpdateRotation"/>.</summary>
	public float angle = 0f;

	/// <summary>
	/// Przesunięcie fazy zapewniające wizualnie poprawne zazębienie zębów.
	/// Obliczane jednorazowo przez <see cref="SnapPhaseWithMotor"/> lub <see cref="SnapPhaseWithGear"/>.
	/// Zerowane przez <see cref="Reset"/>.
	/// </summary>
	public float phaseOffset = 0f;

	/// <summary>
	/// Inicjalizuje <see cref="Radius"/> i <see cref="ToothCount"/> z przypisanego zasobu <see cref="config"/>.
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
	/// Zeruje powiązania w grafie i <see cref="phaseOffset"/>.
	/// Wywoływana przez <see cref="PhysicsEngine.BuildGraph"/> przed każdym przebudowaniem grafu.
	/// </summary>
	public void Reset()
	{
		Parent = null;
		MotorParent = null;
		Children.Clear();
		phaseOffset = 0f;
	}

	/// <summary>
	/// Sprawdza, czy koło może zazębić się z <paramref name="other"/> gdyby znajdowało się na pozycji <paramref name="pos"/>.
	/// Kryterium: <c>|dist − (R1 + R2)| &lt; 0.2</c>.
	/// </summary>
	/// <param name="pos">Hipotetyczna pozycja środka tego koła.</param>
	/// <param name="other">Koło do sprawdzenia.</param>
	/// <returns><c>true</c> jeśli koła zazębią się w tej pozycji.</returns>
	public bool CanMeshAtPos(Vector3 pos, Gear other)
	{
		float dist = pos.DistanceTo(other.GlobalPosition);
		return Mathf.Abs(dist - (Radius + other.Radius)) < 0.2f;
	}

	/// <summary>
	/// Sprawdza, czy koło może zazębić się z silnikiem gdyby znajdowało się na pozycji <paramref name="pos"/>.
	/// Kryterium: <c>|dist − (R + Rmotor)| &lt; 0.2</c>.
	/// </summary>
	/// <param name="pos">Hipotetyczna pozycja środka tego koła.</param>
	/// <param name="motor">Silnik do sprawdzenia.</param>
	/// <returns><c>true</c> jeśli koło zazębi się z silnikiem.</returns>
	public bool CanMeshMotorAtPos(Vector3 pos, Motor motor)
	{
		float dist = pos.DistanceTo(motor.GlobalPosition);
		return Mathf.Abs(dist - (Radius + motor.Radius)) < 0.2f;
	}

	/// <summary>
	/// Sprawdza, czy koło nachodziłoby na <paramref name="other"/> gdyby znajdowało się na pozycji <paramref name="pos"/>.
	/// Kryterium: <c>dist &lt; (R1 + R2) − 0.1</c>.
	/// </summary>
	/// <param name="pos">Hipotetyczna pozycja środka tego koła.</param>
	/// <param name="other">Koło do sprawdzenia.</param>
	/// <returns><c>true</c> jeśli koła nakładałyby się.</returns>
	public bool OverlapsAtPos(Vector3 pos, Gear other)
	{
		float dist = pos.DistanceTo(other.GlobalPosition);
		return dist < (Radius + other.Radius) - 0.1f;
	}

	/// <summary>
	/// Sprawdza, czy koło nachodziłoby na silnik gdyby znajdowało się na pozycji <paramref name="pos"/>.
	/// Kryterium: <c>dist &lt; (R + Rmotor) − 0.1</c>.
	/// </summary>
	/// <param name="pos">Hipotetyczna pozycja środka tego koła.</param>
	/// <param name="motor">Silnik do sprawdzenia.</param>
	/// <returns><c>true</c> jeśli koło nakładałoby się z silnikiem.</returns>
	public bool OverlapsMotorAtPos(Vector3 pos, Motor motor)
	{
		float dist = pos.DistanceTo(motor.GlobalPosition);
		return dist < (Radius + motor.Radius) - 0.1f;
	}

	/// <summary>
	/// Ustawia <paramref name="parent"/> jako koło nadrzędne i rejestruje się w jego liście <see cref="Children"/>.
	/// Wywoływana wyłącznie przez <see cref="PhysicsEngine.BuildGraph"/>.
	/// </summary>
	/// <param name="parent">Koło nadrzędne w grafie przekładni.</param>
	public void SetParent(Gear parent)
	{
		Parent = parent;
		parent.Children.Add(this);
	}

	/// <summary>
	/// Oblicza kąt obrotu tego koła na podstawie przełożenia i propaguje ruch do potomków.
	/// Wzór: <c>angle = -parentAngle * (parentTeeth / thisTeeth) + phaseOffset</c>.
	/// Wywoływana co klatkę przez <see cref="Motor._Process"/> lub koło nadrzędne.
	/// </summary>
public void UpdateRotation()
{
	if (MotorParent != null)
	{
		if (!IsInstanceValid(MotorParent)) return;
		float ratio = (float)MotorParent.ToothCount / (float)ToothCount;
		angle = (-MotorParent.angle * ratio) + phaseOffset;
	}
	else if (Parent != null)
	{
		if (!IsInstanceValid(Parent)) return;
		float ratio = (float)Parent.ToothCount / (float)ToothCount;
		angle = (-Parent.angle * ratio) + phaseOffset;
	}
	else return;

	Rotation = new Vector3(0, angle, 0);

	foreach (var c in Children)
		if (IsInstanceValid(c))
			c.UpdateRotation();
}

	/// <summary>
	/// Oblicza <see cref="phaseOffset"/> tak, aby zęby tego koła trafiały w szczeliny
	/// zębów silnika w punkcie styku. Wywoływana raz po umieszczeniu koła w scenie.
	/// Uwzględnia korekcję <see cref="GearType.AngleOffset"/> z konfiguracji.
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
	/// Oblicza <see cref="phaseOffset"/> tak, aby zęby tego koła trafiały w szczeliny
	/// zębów koła <paramref name="other"/> w punkcie styku.
	/// Wywoływana gdy nowe koło nie zazębia się bezpośrednio z silnikiem.
	/// </summary>
	/// <param name="other">Koło, z którym synchronizowana jest faza.</param>
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

using Godot;
using System.Collections.Generic;

public partial class Gear : Node3D
{
	[Export] public float radius = 1.23f;
	[Export] public int toothCount = 20;

	public Gear Parent;
	public Motor MotorParent;
	public List<Gear> Children = new List<Gear>();

	public float angle = 0f;

	public void Reset()
	{
		Parent = null;
		MotorParent = null;
		Children.Clear();
	}

	// --- проверки ГЕОМЕТРИИ (через позицию, а не через ноду) ---
	public bool CanMeshAtPos(Vector3 pos, Gear other)
	{
		float dist = pos.DistanceTo(other.GlobalPosition);
		return Mathf.Abs(dist - (radius + other.radius)) < 0.05f;
	}

	public bool CanMeshMotorAtPos(Vector3 pos, Motor motor)
	{
		float dist = pos.DistanceTo(motor.GlobalPosition);
		return Mathf.Abs(dist - (radius + motor.Radius)) < 0.05f;
	}

	public bool OverlapsAtPos(Vector3 pos, Gear other)
	{
		float dist = pos.DistanceTo(other.GlobalPosition);
		GD.Print("dist=", dist, " sum=", radius + other.radius);
		return dist < (radius + other.radius) - 0.1f;
		
	}

	public bool OverlapsMotorAtPos(Vector3 pos, Motor motor)
{
	float dist = pos.DistanceTo(motor.GlobalPosition);

	GD.Print("motor dist=", dist, " sum=", radius + motor.Radius);

	return dist < (radius + motor.Radius) - 0.1f;
}

	// --- граф ---
	public void SetParent(Gear parent)
	{
		Parent = parent;
		parent.Children.Add(this);
	}

	// --- вращение ---
	public void UpdateRotation()
	{
	if (Parent != null)
	{
		float ratio = Parent.radius / radius;
		angle = -Parent.angle * ratio;
	}
	else if (MotorParent != null)
	{
		float ratio = MotorParent.Radius / radius;
		angle = -MotorParent.angle * ratio;
	}
	else return;

	Rotation = new Vector3(0, angle, 0);

	foreach (var c in Children)
		c.UpdateRotation();
	}
	public void SnapPhaseWithGear(Gear other)
{
	Vector3 dir = (GlobalPosition - other.GlobalPosition).Normalized();
	float contactAngle = Mathf.Atan2(dir.X, dir.Z);

	float otherTooth = (2f * Mathf.Pi) / other.toothCount;
	float myTooth = (2f * Mathf.Pi) / toothCount;

	float otherPhase = other.Rotation.Y + contactAngle;

	// ставим наш зуб в промежуток другого
	float target = otherPhase + otherTooth * 0.5f;

	angle = -target * (other.radius / radius);
	Rotation = new Vector3(0, angle, 0);
}
public void SnapPhaseWithMotor(Motor motor)
{
	Vector3 dir = (GlobalPosition - motor.GlobalPosition).Normalized();
	float contactAngle = Mathf.Atan2(dir.X, dir.Z);

	float motorTooth = (2f * Mathf.Pi) / motor.ToothCount;

	float motorPhase = motor.Rotation.Y + contactAngle;

	float target = motorPhase + motorTooth * 0.5f;

	angle = -target * (motor.Radius / radius);
	Rotation = new Vector3(0, angle, 0);
}
}

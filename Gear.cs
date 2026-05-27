using Godot;
using System.Collections.Generic;

public partial class Gear : Node3D
{
	[Export] public GearType config;
	[Export] public float Radius = 1.23f;
	[Export] public int ToothCount = 20;

	public Gear Parent;
	public Motor MotorParent;
	public Target TargetParent;
	public List<Gear> Children = new List<Gear>();
	public float angle = 0f;
	public float phaseOffset = 0f;
	public Basis initialBasis = Basis.Identity;
	public Axis PlacedOnAxis = null;

	public override void _Ready()
	{
		initialBasis = Transform.Basis;
		if (config != null)
		{
			Radius = config.Radius;
			ToothCount = config.ToothCount;
		}
	}

public void Reset()
{
	Parent = null;
	MotorParent = null;
	TargetParent = null;
	Children.Clear();
	phaseOffset = 0f;
	angle = 0f; // ← добавить это
	Transform = new Transform3D(initialBasis, Transform.Origin); // ← и это
}

	public void SetParent(Gear parent)
	{
		Parent = parent;
		parent.Children.Add(this);
	}

	private float GetMeshSign(Vector3 myPos, Vector3 parentPos, Basis myBasis, Basis parentBasis)
	{
		Vector3 dir = (myPos - parentPos).Normalized();
		Vector3 v1 = parentBasis.Column1.Normalized().Cross(dir);
		Vector3 v2 = myBasis.Column1.Normalized().Cross(dir);
		float d = v1.Dot(v2);
		if (Mathf.Abs(d) < 0.001f) return -1f;
		return d > 0 ? -1f : 1f;
	}

	public void UpdateRotation()
	{
		if (MotorParent != null)
		{
			if (!IsInstanceValid(MotorParent)) return;
			float ratio = (float)MotorParent.ToothCount / (float)ToothCount;
			float sign = GetMeshSign(GlobalPosition, MotorParent.GlobalPosition, initialBasis, MotorParent.initialBasis);
			angle = (MotorParent.angle * ratio * sign) + phaseOffset;
		}
		else if (Parent != null)
		{
			if (!IsInstanceValid(Parent)) return;
			float ratio = (float)Parent.ToothCount / (float)ToothCount;
			float sign = GetMeshSign(GlobalPosition, Parent.GlobalPosition, initialBasis, Parent.initialBasis);
			angle = (Parent.angle * ratio * sign) + phaseOffset;
		}
		else if (TargetParent != null)
		{
			if (!IsInstanceValid(TargetParent)) return;
			float ratio = (float)TargetParent.ToothCount / (float)ToothCount;
			float sign = GetMeshSign(GlobalPosition, TargetParent.GlobalPosition, initialBasis, TargetParent._initialBasis);
			angle = (TargetParent.angle * ratio * sign) + phaseOffset;
		}
		else return;

		Transform = new Transform3D(
			initialBasis.Rotated(initialBasis.Column1.Normalized(), angle),
			Transform.Origin
		);

		foreach (var c in Children)
			if (IsInstanceValid(c))
				c.UpdateRotation();
	}

	public bool CanMeshAtPos(Vector3 pos, Gear other)
	{
		return Mathf.Abs(pos.DistanceTo(other.GlobalPosition) - (Radius + other.Radius)) < 0.2f;
	}

	public bool CanMeshMotorAtPos(Vector3 pos, Motor motor)
	{
		return Mathf.Abs(pos.DistanceTo(motor.GlobalPosition) - (Radius + motor.Radius)) < 0.2f;
	}

	public bool OverlapsAtPos(Vector3 pos, Gear other)
	{
		return pos.DistanceTo(other.GlobalPosition) < (Radius + other.Radius) - 0.1f;
	}

	public bool OverlapsMotorAtPos(Vector3 pos, Motor motor)
	{
		return pos.DistanceTo(motor.GlobalPosition) < (Radius + motor.Radius) - 0.1f;
	}

	public void SnapPhaseWithMotor(Motor motor)
	{
		Vector3 dir = (GlobalPosition - motor.GlobalPosition).Normalized();
		float contactAngle = Mathf.Atan2(dir.X, dir.Z);
		float motorTooth = (2f * Mathf.Pi) / motor.ToothCount;
		float nearestGap = (Mathf.Floor((motor.angle + contactAngle) / motorTooth) + 0.5f) * motorTooth;
		float sign = GetMeshSign(GlobalPosition, motor.GlobalPosition, initialBasis, motor.initialBasis);
		float desiredAngle = (nearestGap - contactAngle) * (motor.Radius / Radius) * sign;
		phaseOffset = desiredAngle - (motor.angle * (motor.Radius / Radius) * sign);
		angle = desiredAngle;
		Transform = new Transform3D(initialBasis.Rotated(initialBasis.Column1.Normalized(), angle), Transform.Origin);
		angle += config != null ? config.AngleOffset : 0f;
		phaseOffset += config != null ? config.AngleOffset : 0f;
		Transform = new Transform3D(initialBasis.Rotated(initialBasis.Column1.Normalized(), angle), Transform.Origin);
	}

	public void SnapPhaseWithGear(Gear other)
	{
		Vector3 dir = (GlobalPosition - other.GlobalPosition).Normalized();
		float contactAngle = Mathf.Atan2(dir.X, dir.Z);
		float otherTooth = (2f * Mathf.Pi) / other.ToothCount;
		float nearestGap = (Mathf.Floor((other.angle + contactAngle) / otherTooth) + 0.5f) * otherTooth;
		float sign = GetMeshSign(GlobalPosition, other.GlobalPosition, initialBasis, other.initialBasis);
		float desiredAngle = (nearestGap - contactAngle) * (other.Radius / Radius) * sign;
		phaseOffset = desiredAngle - (other.angle * (other.Radius / Radius) * sign);
		angle = desiredAngle;
		Transform = new Transform3D(initialBasis.Rotated(initialBasis.Column1.Normalized(), angle), Transform.Origin);
	}
}

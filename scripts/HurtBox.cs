using Godot;
using System;

public partial class HurtBox : Area2D
{
	[Export]
	public Node2D HurtBoxOwner { get; set; }

	public IDamageable Damageable => HurtBoxOwner as IDamageable;
}

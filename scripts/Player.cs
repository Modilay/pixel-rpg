using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D, IDamageable
{

	private string currentDir = "down";

	[Export]
	public float Health { get; set; } = 100f;
	[Export]
	public float Speed { get; set; } = 100f;
	[Export]
	public float AttackPower { get; set; } = 10f;
	[Export]
	public float KnockbackForce { get; set; } = 40f;

	public bool IsAlive => Health > 0;

	private bool isAttacking = false;
	private bool isHurt = false;
	private float hurtTimer = 0;
	private HashSet<HurtBox> attackEnemies = new HashSet<HurtBox>();

	private AnimationPlayer animationPlayer;
	private AnimatedSprite2D anim;
	private Area2D attackArea;
	private HurtBox hurtBox;
	private ProgressBar healthBar;

	public override void _Ready()
	{
		anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		attackArea = GetNode<Area2D>("AttackArea");
		hurtBox = GetNode<HurtBox>("HurtBox");
		healthBar = GetNode<ProgressBar>("HealthBar");

		attackArea.AreaEntered += OnAttackAreaHit;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		healthBar.Value = Mathf.Clamp(Health, 0, healthBar.MaxValue);
		healthBar.Visible = healthBar.Value < healthBar.MaxValue;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Health > 0)
		{

			if (isHurt)
			{
				hurtTimer -= (float)delta;
				if (hurtTimer <= 0)
				{
					isHurt = false;
					hurtTimer = 0;
					Velocity = Vector2.Zero;
				}
				MoveAndSlide();
				return;
			}

			PlayerAttack();
			PlayerMovement(delta);
		}
	}

	private void PlayerDeath()
	{
		animationPlayer.Stop();
		anim.Play("death");
		isAttacking = false;
		attackEnemies.Clear();
		hurtBox.Monitorable = false;
	}

	private void PlayerAttack()
	{
		if (Input.IsActionJustPressed("attack") && !isAttacking && currentDir != null)
		{
			animationPlayer.Play(currentDir + "_attack");
		}
	}

	private void PlayerMovement(double delta)
	{
		bool movement = false;
		var velocity = Vector2.Zero;

		if (!isAttacking)
		{
			if (Input.IsActionPressed("ui_down"))
			{
				currentDir = "down";
				velocity = Vector2.Down * Speed;
				movement = true;
			}
			else if (Input.IsActionPressed("ui_up"))
			{
				currentDir = "up";
				velocity = Vector2.Up * Speed;
				movement = true;
			}
			else if (Input.IsActionPressed("ui_right"))
			{
				currentDir = "right";
				velocity = Vector2.Right * Speed;
				movement = true;
			}
			else if (Input.IsActionPressed("ui_left"))
			{
				currentDir = "left";
				velocity = Vector2.Left * Speed;
				movement = true;
			}

		}

		Velocity = velocity;
		AnimmPlay(movement);

		MoveAndSlide();
	}

	private void AnimmPlay(bool movement)
	{
		if (isAttacking)
			return;
		bool filp = false;
		string animName = null;
		switch (currentDir)
		{
			case "left":
			case "right":
				animName = "side_";
				filp = currentDir == "left";
				break;
			case "up":
				animName = "back_";
				break;
			case "down":
				animName = "front_";
				break;
		}
		if (movement)
			animName += "walk";
		else
			animName += "idle";
		anim.FlipH = filp;
		anim.Play(animName);
	}

	public void AttackAnimationStart()
	{
		attackEnemies.Clear();
		isAttacking = true;
		GD.Print("玩家攻击开始");
	}

	public void AttackAnimationEnd()
	{
		isAttacking = false;
		attackEnemies.Clear();
		GD.Print("玩家攻击结束");
	}

	private void OnAttackAreaHit(Area2D area)
	{
		if (area is HurtBox hurtBox && hurtBox.HurtBoxOwner != this && !attackEnemies.Contains(hurtBox))
		{
			attackEnemies.Add(hurtBox);

			var dir = (hurtBox.HurtBoxOwner.Position - this.Position).Normalized();

			hurtBox.Damageable?.TakeDamage(AttackPower, KnockbackForce, dir, this, 0.1f);
		}
	}

	public DamageResult TakeDamage(float damage, float knockbackForce = 0f, Vector2? knockbackDirection = null, Node2D damageSource = null, float hurtDuration = 0.2f)
	{
		var result = DamageResult.Damaged;
		if (Health > 0)
		{
			Health -= damage;
			GD.Print($"{Name}受到 {damageSource?.Name ?? "未知来源"} 的 {damage}点伤害，剩余生命: {Health}");
			if (Health <= 0)
			{
				PlayerDeath();
				result = DamageResult.Death;
			}

			Velocity = (knockbackDirection ?? Vector2.Zero) * knockbackForce;
			isHurt = true;
			hurtTimer = hurtDuration;
			return result;
		}

		return DamageResult.None;
	}
}

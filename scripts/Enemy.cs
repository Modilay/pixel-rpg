using Godot;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;

public partial class Enemy : CharacterBody2D, IDamageable
{
	[Export]
	public float Health { get; set; } = 60f;
	[Export]
	public float Speed { get; set; } = 70f;
	[Export]
	public float AttackPower { get; set; } = 15f;
	[Export]
	public float KnockbackForce { get; set; } = 50f;
	[Export]
	public float AttackOffsetForce { get; set; } = 80f;
	[Export]
	public Vector2 SpawnPos { get; set; } = Vector2.Zero;

    public bool IsAlive => Health > 0;

    // -------------------------------------------------
    private AnimationPlayer animationPlayer;
	private AnimatedSprite2D anim;
	private Area2D detectArea;
	private Area2D attackRange;
	private Area2D attackArea;
	private Area2D hurtBox;
	private ProgressBar healthBar;

	// -------------------------------------------

	private Node2D target;
	private Vector2 attackDir = Vector2.Zero;
	private bool ischasing = false;
	private bool isAttacking = false;
	private bool isHurt = false;
	private float hurtTimer = 0;
	private HashSet<HurtBox> attackEnemies = new HashSet<HurtBox>();
	private List<HurtBox> chaseTargets = new List<HurtBox>();
	private HashSet<Node2D> attackRangeTargets = new HashSet<Node2D>();


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		SpawnPos = Position;

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		detectArea = GetNode<Area2D>("DetectionArea");
		attackRange = GetNode<Area2D>("AttackRange");
		attackArea = GetNode<Area2D>("AttackArea");
		hurtBox = GetNode<Area2D>("HurtBox");
		healthBar = GetNode<ProgressBar>("HealthBar");

		detectArea.AreaEntered += AreaDetectEnter;
		detectArea.AreaExited += AreaDetectExit;

		attackRange.AreaEntered += AttackRangeEnter;
		attackRange.AreaExited += AttackRangeExit;

		attackArea.AreaEntered += AttackAreaEntered;
	}

	public override void _Process(double delta)
    {
        base._Process(delta);
		healthBar.Value = Mathf.Clamp(Health, 0, healthBar.MaxValue);
		healthBar.Visible = healthBar.Value < healthBar.MaxValue;
    }

	public override void _PhysicsProcess(double delta)
	{
		if (Health <= 0)
			return;
		if (isHurt)
		{
			hurtTimer -= (float)delta;
			if (hurtTimer <= 0)
			{
				isHurt = false;
				hurtTimer = 0;
				Velocity = Vector2.Zero;
			}
		}
		else
		{
			if (!isAttacking)
			{
				if (ischasing && target != null)
				{
					attackDir = (target.Position - this.Position).Normalized();
					Velocity = attackDir * Speed;
				}

				if (attackRangeTargets.Contains(target))
				{
					animationPlayer.Play("attack");
				}
				else
				{
					AnimaPlay();
				}
			}
		}

		MoveAndSlide();
	}

	private void AnimaPlay()
	{
		if(Health < 0)
			return;
		if (ischasing)
		{
			anim.Play("walk");
			if (Velocity.X > 0)
			{
				anim.FlipH = false;
			}
			else if (Velocity.X < 0)
			{
				anim.FlipH = true;
			}
		}
		else
		{
			anim.Play("idle");
		}
	}

	private void AreaDetectEnter(Area2D area)
	{
		if (area is HurtBox hurtBox && hurtBox.HurtBoxOwner != this && hurtBox.Damageable?.IsAlive == true)
		{
			GD.Print(hurtBox.HurtBoxOwner.Name + "已进入侦测区域");
			chaseTargets.Add(hurtBox);
			if (target == null)
			{
				target = hurtBox.HurtBoxOwner;
				ischasing = true;
			}
		}
	}

	private void AreaDetectExit(Area2D area)
	{
		if (area is HurtBox hurtBox && hurtBox.Damageable?.IsAlive == true)
		{
			if (chaseTargets.Contains(hurtBox))
			{
				GD.Print(hurtBox.HurtBoxOwner.Name + "已离开侦测区域");
				chaseTargets.Remove(hurtBox);
				if (target == hurtBox.HurtBoxOwner)
				{
					target = chaseTargets.Count > 0 ? chaseTargets[0].HurtBoxOwner : null;
					ischasing = target != null;
				}
			}

		}
	}

	private void AttackRangeEnter(Area2D area)
	{
		if(area is HurtBox hurtbox  && hurtbox.HurtBoxOwner != null && hurtbox.HurtBoxOwner != this && hurtbox.Damageable?.IsAlive == true)
		{
			attackRangeTargets.Add(hurtbox.HurtBoxOwner);
		}
	}

	private void AttackRangeExit(Area2D area)
	{
		if(area is HurtBox hurtbox && hurtbox.HurtBoxOwner != null && hurtbox.HurtBoxOwner != this)
		{
			attackRangeTargets.Remove(hurtbox.HurtBoxOwner);
		}
	}

	public void AttackAnimaStart()
	{
		attackEnemies.Clear();
		isAttacking = true;
		GD.Print("攻击动画开始，准备攻击");
		Velocity = attackDir * AttackOffsetForce;
	}
	public void AttackOffsetEnd()
	{
		Velocity = Vector2.Zero;
	}

	public void AttackEnd()
	{
		isAttacking = false;
		attackEnemies.Clear();
		GD.Print("攻击动画结束，重置状态");
	}

	private void AttackAreaEntered(Area2D area)
	{
		if (area is HurtBox hurtBox && hurtBox.HurtBoxOwner != this && hurtBox.Damageable?.IsAlive == true)
		{
			var dir = (hurtBox.HurtBoxOwner.Position - this.Position).Normalized();
			var result = hurtBox.Damageable?.TakeDamage(AttackPower, KnockbackForce, dir, this,0.1f);
			if(result == DamageResult.Death)
			{
				attackRangeTargets.Remove(hurtBox.HurtBoxOwner);
				attackEnemies.Remove(hurtBox);
				chaseTargets.Remove(hurtBox);
				if (target == hurtBox.HurtBoxOwner)
				{
					target = chaseTargets.Count > 0 ? chaseTargets[0].HurtBoxOwner : null;
					ischasing = target != null;
				}
			}
		}
	}

	public DamageResult TakeDamage(float damage, float knockbackForce = 0, Vector2? knockbackDirection = null, Node2D damageSource = null, float hurtDuration = 0.2f)
	{
		var result = DamageResult.Damaged;
		if (Health > 0)
		{
			Health -= damage;
			GD.Print($"{Name}受到 {damageSource?.Name ?? "未知来源"} 的 {damage}点伤害，剩余生命: {Health}");
			if (Health <= 0)
			{
				Death();
				result = DamageResult.Death;
			}

			Velocity = (knockbackDirection ?? Vector2.Zero) * knockbackForce;
			isHurt = true;
			hurtTimer = hurtDuration;
			return result;

		}
		return DamageResult.None;
	}

	private void Death()
	{
		animationPlayer.Stop();
		anim.Play("death");
		isAttacking = false;
		ischasing = false;
		attackEnemies.Clear();

		attackRangeTargets.Clear();
		chaseTargets.Clear();
		target = null;

		hurtBox.Monitorable = false;
		detectArea.Monitoring = false;
		attackRange.Monitoring = false;
	}
}

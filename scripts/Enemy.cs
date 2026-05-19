using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export]
	public float Speed { get; set; } = 70f;

	private AnimatedSprite2D anim;
	private Area2D detectArea;

	private Player player;

	private bool ischasing = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		detectArea = GetNode<Area2D>("DetectionArea");

		detectArea.BodyEntered += BodyDetectEnter;
		detectArea.BodyExited += BodyDetectExit;

	}

	public override void _PhysicsProcess(double delta)
	{
		if (ischasing)
		{
			Velocity = (player.Position - this.Position).Normalized() * Speed;
		}
		else
		{
			Velocity = Vector2.Zero;
		}
		AnimaPlay();
		MoveAndSlide();
	}

	private void AnimaPlay()
	{
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

	private void BodyDetectEnter(Node2D body)
	{
		if (body is Player p)
		{
			this.player = p;
			this.ischasing = true;
		}
	}

	private void BodyDetectExit(Node2D body)
	{
		if (body is Player p && p == this.player)
		{
			this.player = null;
			this.ischasing = false;
		}
	}



}

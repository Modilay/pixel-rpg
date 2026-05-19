using Godot;
using System;

public partial class Player : CharacterBody2D
{

	private string currentDir = "down";

	[Export]
	public float Speed { get; set; } = 100f;

	private bool isAttacking = false;

	private AnimationPlayer animationPlayer;
	private AnimatedSprite2D anim;
	public override void _Ready()
	{
		anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}


	public override void _PhysicsProcess(double delta)
	{
		PlayerAttack();
		PlayerMovement(delta);
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
		isAttacking = true;
	}

	public void AttackAnimationEnd()
	{
		isAttacking = false;
	}
}

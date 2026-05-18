using Godot;
using System;

public partial class Player : CharacterBody2D
{
	
	private string currentDir = null;

	[Export]
	public float Speed {get; set;} = 100f;

	private AnimatedSprite2D anim;
	public override void _Ready()
	{
		anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}


    public override void _PhysicsProcess(double delta)
    {
        PlayerMovement(delta);
    }

	private void PlayerMovement(double delta)
	{
		bool movement = true;
		var velocity = Velocity;
		if (Input.IsActionPressed("ui_right"))
		{
			currentDir = "right";
			velocity.X = Speed;
			velocity.Y = 0;
		}else if (Input.IsActionPressed("ui_left"))
		{
			currentDir = "left";
			velocity.X = -Speed;
			velocity.Y = 0;
		}else if (Input.IsActionPressed("ui_down"))
		{
			currentDir = "down";
			velocity.X = 0;
			velocity.Y = Speed;
		}else if (Input.IsActionPressed("ui_up"))
		{
			currentDir = "up";
			velocity.X = 0;
			velocity.Y = -Speed;
		}else
		{
			velocity.X = 0;
			velocity.Y = 0;
			movement = false;
		}
		AnimmPlay(movement);
		
		Velocity = velocity;
		MoveAndSlide();
	}

	private void AnimmPlay(bool movement)
	{
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
}

using Godot;
using System;

public partial class Portal : Area2D
{

	[Export]
	public WorldEnum world;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.BodyEntered += BodyEnteredHandler;
	}

	private void BodyEnteredHandler(Node2D body)
	{
		if(body is Player)
		{
			CallDeferred(nameof(GotoWorld));
		}
	}

	private void GotoWorld()
	{
		GetNode<Global>("/root/Global").ChangeWorld(world);
	}
}

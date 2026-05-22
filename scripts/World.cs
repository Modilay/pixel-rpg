using Godot;
using System;

public partial class World : Node2D
{
	[Export]
	public Camera2D camera2D;

	[Export]
	public Player player;
	public override void _Ready()
	{
		Global global = GetNode<Global>("/root/Global");
		if(global.CurrentWorld == WorldEnum.World)
		{
			if (!global.WorldInit)
			{
				player.Position = new Vector2(200, 15);
			}
		}

		camera2D.GetParent().RemoveChild(camera2D);
		player.AddChild(camera2D);
		camera2D.Position = Vector2.Zero;
	}
}

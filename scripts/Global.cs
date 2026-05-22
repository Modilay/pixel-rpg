using Godot;
using System;

public partial class Global : Node
{
	public WorldEnum CurrentWorld { get; set; } = WorldEnum.World;
    public bool WorldInit = true;

    public bool ChangeWorld(WorldEnum newWorld)
    {
        if (CurrentWorld == newWorld)
            return false;

        WorldInit = false;
        GetTree().ChangeSceneToFile(newWorld.ToScenePath());
        CurrentWorld = newWorld;
        return true;
    }
}

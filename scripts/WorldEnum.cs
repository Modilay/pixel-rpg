public enum WorldEnum
{
    World = 0,
    Cliff = 1
}


public static class WorldEnumExtensions
{
    public static string ToScenePath(this WorldEnum world)
    {
        return world switch
        {
            WorldEnum.World => "res://scenes/world.tscn",
            WorldEnum.Cliff => "res://scenes/cliff_side.tscn",
            _ => null
        };
    }
}
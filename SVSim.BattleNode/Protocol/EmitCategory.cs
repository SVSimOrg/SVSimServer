namespace SVSim.BattleNode.Protocol;

/// <summary>The "cat" field on the msg envelope.</summary>
public enum EmitCategory
{
    Battle = 1,
    Matching = 2,
    Room = 3,
    Watch = 11,
    General = 99,
}

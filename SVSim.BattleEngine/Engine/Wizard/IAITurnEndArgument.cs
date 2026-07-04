namespace Wizard;

public interface IAITurnEndArgument
{
	bool IsAllyTurn { get; }

	float CalculateThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allInplayStatusList);
}

namespace Wizard;

public class PlaySimulationInfo
{
	public AIVirtualCard Actor;

	public int UseCost;

	public PlaySimulationType Type;

	public PlaySimulationInfo(AIVirtualCard actor, int cost, PlaySimulationType type)
	{
		Actor = actor;
		UseCost = cost;
		Type = type;
	}
}

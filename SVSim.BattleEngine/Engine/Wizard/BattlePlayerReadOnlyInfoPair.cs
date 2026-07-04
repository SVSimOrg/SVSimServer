namespace Wizard;

public class BattlePlayerReadOnlyInfoPair
{
	public IBattlePlayerReadOnlyInfo ReadOnlySelf { get; protected set; }

	public IBattlePlayerReadOnlyInfo ReadOnlyOpponent { get; protected set; }

	public BattlePlayerReadOnlyInfoPair(IBattlePlayerReadOnlyInfo self, IBattlePlayerReadOnlyInfo opponent)
	{
		ReadOnlySelf = self;
		ReadOnlyOpponent = opponent;
	}
}

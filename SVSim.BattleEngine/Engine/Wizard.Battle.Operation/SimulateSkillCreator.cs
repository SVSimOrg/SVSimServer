using Wizard.Battle.Card;
using Wizard.Battle.Resource;
// TODO(engine-cleanup-pass2): 2 of 3 methods unrun in baseline
//   Type: Wizard.Battle.Operation.SimulateSkillCreator
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Operation;

public class SimulateSkillCreator : SkillCreator
{
	public SimulateSkillCreator(BattleCardBase ownerCard, BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer, IBattleResourceMgr battleResourceMgr)
		: base(ownerCard, selfBattlePlayer, opponentBattlePlayer, battleResourceMgr)
	{
	}

	protected override ISkillSelectFilter CreateRandomSelectFilter(string count)
	{
		return new SimulateRandomSelectFilter((IVirtualBattleCard)_ownerCard, count);
	}

	protected override ISkillSelectFilter CreateIdNoDuplicationRandomSelectFilter(string count)
	{
		return new SimulateIdNoDuplicationRandomSelectFilter((IVirtualBattleCard)_ownerCard, count);
	}

	protected override ISkillSelectFilter CreateCostNoDuplicationRandomSelectFilter(string count)
	{
		return new SimulateCostNoDuplicationRandomSelectFilter((IVirtualBattleCard)_ownerCard, count);
	}
}

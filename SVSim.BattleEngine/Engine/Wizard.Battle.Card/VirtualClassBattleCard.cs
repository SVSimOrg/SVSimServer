using Wizard.Battle.Operation;
using Wizard.Battle.Resource;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 3 of 7 methods unrun in baseline
//   Type: Wizard.Battle.Card.VirtualClassBattleCard
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Card;

public class VirtualClassBattleCard : ClassBattleCardBase, IVirtualBattleCard
{
	public override int Index => 0;

	public bool UsedRandomSkill { get; set; }

	public VirtualClassBattleCard(ClassBuildInfo buildInfo)
		: base(buildInfo)
	{
	}

	protected override IBattleCardView CreateView(BattleCardView.BuildInfo buildInfo, bool isNullView)
	{
		return new NullClassBattleCardView(buildInfo);
	}


	protected override void _CacheBattlePlayer()
	{
	}

	public override SkillCreator CreateSkillCreator(BattlePlayerBase selfBattlPlayer, BattlePlayerBase opponentBattlePlayer, IBattleResourceMgr resourceMgr)
	{
		return new SimulateSkillCreator(this, selfBattlPlayer, opponentBattlePlayer, _buildInfo.ResourceMgr);
	}
}

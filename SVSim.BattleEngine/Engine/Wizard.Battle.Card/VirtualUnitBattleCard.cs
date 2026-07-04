using Wizard.Battle.Operation;
using Wizard.Battle.Resource;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 5 of 8 methods unrun in baseline
//   Type: Wizard.Battle.Card.VirtualUnitBattleCard
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Card;

public class VirtualUnitBattleCard : UnitBattleCard, IVirtualBattleCard
{
	public bool UsedRandomSkill { get; set; }

	public VirtualUnitBattleCard(BuildInfo buildInfo)
		: base(buildInfo)
	{
	}

	public override VfxBase TurnStart(SkillProcessor skillProcessor)
	{
		return base.TurnStart(skillProcessor);
	}

	protected override IBattleCardView CreateView(BattleCardView.BuildInfo buildInfo, bool isNullView)
	{
		return new NullBattleCardView(buildInfo);
	}


	public override SkillCreator CreateSkillCreator(BattlePlayerBase selfBattlPlayer, BattlePlayerBase opponentBattlePlayer, IBattleResourceMgr resourceMgr)
	{
		return new SimulateSkillCreator(this, selfBattlPlayer, opponentBattlePlayer, _buildInfo.ResourceMgr);
	}

	public override VfxBase LoadResource(bool isLogging = false)
	{
		return NullVfx.GetInstance();
	}

	public override VfxBase UnloadResource()
	{
		return NullVfx.GetInstance();
	}
}

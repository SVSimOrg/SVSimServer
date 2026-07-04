using Wizard.Battle.Operation;
using Wizard.Battle.Resource;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Card;

public class VirtualSpecialSkillBattleCard : SpecialSkillBattleCard, IVirtualBattleCard
{
	public bool UsedRandomSkill { get; set; }

	public VirtualSpecialSkillBattleCard(BossRushSpecialSkill skill, BuildInfo buildInfo)
		: base(skill, buildInfo)
	{
	}

	protected override IBattleCardView CreateView(BattleCardView.BuildInfo buildInfo, bool isNullView)
	{
		return new NullBattleCardView(buildInfo);
	}


	public override SkillCreator CreateSkillCreator(BattlePlayerBase selfBattlPlayer, BattlePlayerBase opponentBattlePlayer, IBattleResourceMgr resourceMgr)
	{
		return new SimulateSkillCreator(this, selfBattlPlayer, opponentBattlePlayer, _buildInfo.ResourceMgr);
	}
}

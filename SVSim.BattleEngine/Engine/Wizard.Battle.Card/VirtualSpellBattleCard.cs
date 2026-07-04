using Wizard.Battle.Operation;
using Wizard.Battle.Resource;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Card;

public class VirtualSpellBattleCard : SpellBattleCard, IVirtualBattleCard
{
	public bool UsedRandomSkill { get; set; }

	public VirtualSpellBattleCard(BuildInfo buildInfo, bool isChoiceBrave)
		: base(buildInfo, isChoiceBrave)
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

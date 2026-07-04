using Wizard.Battle.Operation;
using Wizard.Battle.Resource;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Card;

public class VirtualChantFieldBattleCard : ChantFieldBattleCard, IVirtualBattleCard
{
	public bool UsedRandomSkill { get; set; }

	public VirtualChantFieldBattleCard(BuildInfo buildInfo)
		: base(buildInfo)
	{
	}

	protected override IBattleCardView CreateView(BattleCardView.BuildInfo buildInfo, bool isNullView)
	{
		return new NullFieldBattleCardView(buildInfo);
	}


	public override SkillCreator CreateSkillCreator(BattlePlayerBase selfBattlPlayer, BattlePlayerBase opponentBattlePlayer, IBattleResourceMgr resourceMgr)
	{
		return new SimulateSkillCreator(this, selfBattlPlayer, opponentBattlePlayer, _buildInfo.ResourceMgr);
	}
}

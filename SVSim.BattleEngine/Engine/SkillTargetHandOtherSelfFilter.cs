using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetHandOtherSelfFilter : ISkillTargetFilter
{
	private readonly IReadOnlyBattleCardInfo _ownerCard;

	public SkillTargetHandOtherSelfFilter(IReadOnlyBattleCardInfo ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		BattleCardBase originalCard = ((_ownerCard.TransformInfo.Type != BattleCardBase.TransformType.Metamorphose) ? _ownerCard.TransformInfo.OriginalCard : null);
		return from c in battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoHandCards)
			where c != _ownerCard && c != originalCard
			select c;
	}
}

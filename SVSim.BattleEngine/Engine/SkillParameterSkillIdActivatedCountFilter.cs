using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterSkillIdActivatedCountFilter : ISkillParameterSelectFilter
{
	private long _skillId;

	public SkillParameterSkillIdActivatedCountFilter(long skillId)
	{
		_skillId = skillId;
	}

	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => c.SkillActivationList.Count((BattleCardBase.SkillActivationInfo s) => s.SkillId == _skillId));
	}
}

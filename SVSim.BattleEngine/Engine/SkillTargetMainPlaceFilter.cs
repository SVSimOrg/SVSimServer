using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetMainPlaceFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		list.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoDeckCards));
		list.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoHandCards));
		list.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoClassAndInPlayCards));
		list.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoCemeterys));
		list.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoBanishCards));
		return list;
	}
}

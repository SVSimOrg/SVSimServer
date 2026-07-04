using System.Collections.Generic;
using Wizard.Battle;

public class SkillTurnDestroyedFilter : ISkillTargetFilter
{
	private readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillTurnDestroyedFilter(string option)
	{
		_turnPlayerInfo = new TurnPlayerInfo(option);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		return GetTurnCard(battlePlayerInfos);
	}

	private IEnumerable<IReadOnlyBattleCardInfo> GetTurnCard(IEnumerable<IBattlePlayerReadOnlyInfo> playerInfo)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IBattlePlayerReadOnlyInfo item in playerInfo)
		{
			foreach (IReadOnlyBattleCardInfo specificTurnDestroyCard in item.GetSpecificTurnDestroyCards(_turnPlayerInfo))
			{
				list.Add(specificTurnDestroyCard);
			}
		}
		return list;
	}
}

using System;
using System.Linq;

public class SkillEnvironmentalGameConditionFulfilledTurnPlaycount : ISkillEnvironmentalFilter
{
	private readonly int _conditionPlayCount;

	private readonly Func<int, int, bool> _compareFunc;

	public SkillEnvironmentalGameConditionFulfilledTurnPlaycount(string value, string op)
	{
		_compareFunc = SkillCompareFuncCreator.Create(op);
		_conditionPlayCount = int.Parse(value);
	}

	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return playerInfo.TurnPlayCardCountInfo.Where((TurnAndIntValue c) => c.IsSelfTurn == playerInfo.IsPlayer && _compareFunc(c.Value, _conditionPlayCount)).Count();
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerInfo, option);
	}
}

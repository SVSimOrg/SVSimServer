using System;
using System.Collections.Generic;
using System.Linq;

public class SkillCostNoDuplicationRandomSelectFilter : ISkillSelectFilter
{
	private readonly string _contText;

	private int _count;

	public SkillCostNoDuplicationRandomSelectFilter(string randomCountText)
	{
		_contText = randomCountText;
	}

	public int CalcCount(SkillOptionValue option)
	{
		return option.ParseInt(_contText);
	}

	public IEnumerable<BattleCardBase> Filtering(IEnumerable<BattleCardBase> cards, SkillOptionValue option, SkillConditionCheckerOption checkerOption)
	{
		_count = CalcCount(option);
		cards = cards.OrderBy((BattleCardBase x) => x.Index);
		List<BattleCardBase> cardList = cards.ToList();
		BattleManagerBase battleMgr = cardList.FirstOrDefault()?.SelfBattlePlayer?.BattleMgr;
		_count = Math.Min(_count, cardList.Count);
		for (int i = 0; i < _count; i++)
		{
			if (cardList.Count > 0)
			{
				int index = (battleMgr.InstanceIsRandomDraw ? battleMgr.StableRandom(cardList.Count) : 0);
				BattleCardBase card = cardList[index];
				cardList = cardList.Where((BattleCardBase c) => c.Card.Cost != card.Cost).ToList();
				yield return card;
			}
		}
	}

	public bool IsUpperLimit()
	{
		return _contText == "5-{me.inplay.unit_and_allfield.count}";
	}
}

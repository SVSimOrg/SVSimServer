using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle;

public class SkillExclutionApplyFilter : ISkillExclutionFilter
{
	private ApplySkillTargetFilterCollection _filters = new ApplySkillTargetFilterCollection();

	private BattlePlayerReadOnlyInfoPair _pair;

	public IEnumerable<BattleCardBase> Targets { get; private set; }

	public bool IsExcludeSelectCard => _filters.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.TargetFilter is SkillTargetSelectedCardsFilter);

	public SkillExclutionApplyFilter(BattleCardBase ownerCard, string text, SkillBase skill)
	{
		if (text.First() == '{')
		{
			text = text.Remove(0, 1);
			text = text.Remove(text.Length - 1, 1);
		}
		string[] array = text.Split('&');
		for (int i = 0; i < array.Length; i++)
		{
			SkillFilterCreator.SetupTarget(_filters, array[i], ownerCard, skill);
		}
		_pair = new BattlePlayerReadOnlyInfoPair(ownerCard.SelfBattlePlayer, ownerCard.OpponentBattlePlayer);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption checkerOption, SkillOptionValue optionValue)
	{
		List<IReadOnlyBattleCardInfo> list = cards.ToList();
		Targets = _filters.Filtering(_pair, checkerOption, optionValue).Cast<BattleCardBase>();
		for (int i = 0; i < Targets.Count(); i++)
		{
			list.Remove(Targets.ElementAt(i));
		}
		return list;
	}
}

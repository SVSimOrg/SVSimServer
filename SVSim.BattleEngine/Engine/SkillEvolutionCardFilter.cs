using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillEvolutionCardFilter : ISkillCardFilter
{
	public readonly bool _isEvolution;

	public SkillEvolutionCardFilter(bool isEvolution)
	{
		_isEvolution = isEvolution;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => c.IsEvolution == _isEvolution);
	}
}

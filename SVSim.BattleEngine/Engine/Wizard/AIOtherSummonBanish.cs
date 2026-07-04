using System.Collections.Generic;

namespace Wizard;

public class AIOtherSummonBanish : AITriggerAndTargetFiltersTagBase
{

	public AIScriptTokenArgType SelectType { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIOtherSummonBanish(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 1], base.LegalSelectTypes);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			if (SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIBanishSimulationUtility.BanishAll(targets, situation);
			}
			else if (SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				AIBanishSimulationUtility.BanishRandom(targets, tagOwner, field, playPtn, situation);
			}
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT
		};
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}

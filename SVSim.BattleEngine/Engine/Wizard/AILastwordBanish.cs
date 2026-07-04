using System.Collections.Generic;

namespace Wizard;

public class AILastwordBanish : AIFiltersAndSelectTypeArgument
{
	public AILastwordBanish(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIBanishSimulationUtility.BanishAll(targetsFromField, situation);
			}
			else if (base.SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				AIBanishSimulationUtility.BanishRandom(targetsFromField, tagOwner, field, playPtn, situation);
			}
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}

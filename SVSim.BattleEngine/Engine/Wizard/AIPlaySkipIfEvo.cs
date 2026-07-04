using System.Collections.Generic;

namespace Wizard;

public class AIPlaySkipIfEvo : AIPlaySkipWithFilteredTargets
{
	public AIPlaySkipIfEvo(string text)
		: base(text)
	{
	}

	public override PlaySkipInformation CreatePlaySkipInformation(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		PlaySkipIfEvoInformation playSkipIfEvoInformation = new PlaySkipIfEvoInformation();
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null)
		{
			playSkipIfEvoInformation.AddEvolutionPermittedCards(targetsFromField);
		}
		return playSkipIfEvoInformation;
	}
}

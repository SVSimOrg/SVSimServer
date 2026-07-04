using System.Collections.Generic;

namespace Wizard;

public class AIPlaySkipWithEvo : AIPlaySkipWithFilteredTargets
{
	public AIPlaySkipWithEvo(string text)
		: base(text)
	{
	}

	public override PlaySkipInformation CreatePlaySkipInformation(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		PlaySkipWithEvoInformation playSkipWithEvoInformation = new PlaySkipWithEvoInformation();
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null)
		{
			playSkipWithEvoInformation.AddEvolutionPermittedCards(targetsFromField);
		}
		return playSkipWithEvoInformation;
	}
}

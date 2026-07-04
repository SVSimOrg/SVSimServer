using System.Collections.Generic;

namespace Wizard;

public class AIGetOffEvo : AIFiltersAndSelectTypeArgument
{

	protected override int SELECT_TYPE_OFFSET => 1;

	public AIGetOffEvo(string text)
		: base(text)
	{
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		AIAutoEvolutionSimulationUtility.AutoEvolution(field, targetsFromField, playPtn, situation, base.SelectType);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}

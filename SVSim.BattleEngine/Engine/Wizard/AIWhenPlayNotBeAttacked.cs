using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayNotBeAttacked : AIWhenPlayTagArgument
{
	protected override bool _isSelectCountImplemented => true;

	public AIWhenPlayNotBeAttacked(string text)
		: base(text)
	{
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.TARGET_SELECT
		};
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AINotBeAttackedSimulationUtility.GiveNotBeAttackedToAll(targetsFromField);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
				AINotBeAttackedSimulationUtility.GiveNotBeAttackedToTargeted(situation, base.SelectType);
				break;
			default:
				AIConsoleUtility.LogError("AIWhenPlayNotBeAttacked.Execute() Ileagal SelectType:" + base.SelectType.ToString() + " tagOwner:" + tagOwner.CardName);
				break;
			}
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}

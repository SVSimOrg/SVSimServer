using System.Collections.Generic;

namespace Wizard;

public class AIChangeInplayImmediateIndestructible : AIWhenChangeInplayTagArgument
{
	public AIChangeInplayImmediateIndestructible(string text)
		: base(text, isImmediate: true)
	{
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	protected override void ChangeInplayTagStartProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			GiveIndestructibleToAll(targets);
		}
	}

	private void GiveIndestructibleToAll(List<AIVirtualCard> targets)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			targets[i].IsIndestructible = true;
		}
	}
}

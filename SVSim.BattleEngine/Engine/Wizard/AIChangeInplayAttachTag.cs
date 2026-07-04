using System.Collections.Generic;

namespace Wizard;

public class AIChangeInplayAttachTag : AIWhenChangeInplayTagArgument
{
	private AIScriptTokenArgType _removeTiming;

	private readonly int REMOVE_TIMING_OFFSET = 1;

	public AIPlayTag Tag { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIChangeInplayAttachTag(string text, bool isImmediate)
		: base(text, isImmediate)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		base.InitExpressions(list[0]);
		_removeTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - REMOVE_TIMING_OFFSET]);
		Tag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[1], list[2], list[3]);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}

	protected override void ChangeInplayTagStartProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AIAttachTagSimulationUtility.SimulateAttachTagToAll(targets, tagOwner, Tag, _removeTiming, situation);
		}
	}
}

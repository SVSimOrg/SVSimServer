using System.Collections.Generic;

namespace Wizard;

public class AIBreakAttachTag : AITriggerAndTargetFiltersTagBase
{

	public AIPlayTag AttachedTag { get; private set; }

	public AIScriptTokenArgType RemoveTiming { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIBreakAttachTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		base.InitExpressions(list[0]);
		RemoveTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 1]);
		if (list.Count <= AIPlayTag.TAG_WORDS_LENTGH)
		{
			AttachedTag = null;
		}
		else
		{
			AttachedTag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[1], list[2], list[3]);
		}
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			AIAttachTagSimulationUtility.SimulateAttachTagToAll(targets, tagOwner, AttachedTag, RemoveTiming, situation);
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}

	public override AITokenIdCollection GetAllRegisterTokenPoolInfo(AIVirtualCard owner)
	{
		if (AttachedTag != null)
		{
			return AttachedTag.ArgumentExpressions.GetAllRegisterTokenPoolInfo(owner);
		}
		return null;
	}
}

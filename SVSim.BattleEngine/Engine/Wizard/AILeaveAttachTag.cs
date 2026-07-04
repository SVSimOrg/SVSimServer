using System.Collections.Generic;

namespace Wizard;

public class AILeaveAttachTag : AIFiltersAndSelectTypeArgument
{
	private readonly int TAG_WORD_START_INDEX = 1;

	private readonly int REMOVE_TIMING_OFFSET = 1;

	public AIPlayTag AttachedTag { get; private set; }

	public AIScriptTokenArgType RemoveTiming { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AILeaveAttachTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		base.InitExpressions(list[0]);
		RemoveTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - REMOVE_TIMING_OFFSET]);
		AttachedTag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[TAG_WORD_START_INDEX], list[TAG_WORD_START_INDEX + 1], list[TAG_WORD_START_INDEX + 2]);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			AIAttachTagSimulationUtility.SimulateAttachTagToAll(targetsFromField, tagOwner, AttachedTag, RemoveTiming, situation);
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
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

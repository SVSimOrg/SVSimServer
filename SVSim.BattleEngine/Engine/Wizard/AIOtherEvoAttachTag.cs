using System.Collections.Generic;

namespace Wizard;

public class AIOtherEvoAttachTag : AIOtherEvoTagArgument
{
	private AIScriptTokenArgType _removeTiming;

	private readonly int REMOVE_TIMING_OFFSET = 1;

	public AIPlayTag Tag { get; private set; }

	protected override int SELECT_TYPE_ARG_OFFSET => 2;

	public AIOtherEvoAttachTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		base.InitExpressions(list[0]);
		_removeTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - REMOVE_TIMING_OFFSET]);
		Tag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[1], list[2], list[3]);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AIAttachTagSimulationUtility.SimulateAttachTagToAll(targets, tagOwner, Tag, _removeTiming, situation);
		}
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	public override AITokenIdCollection GetAllRegisterTokenPoolInfo(AIVirtualCard owner)
	{
		if (Tag != null)
		{
			return Tag.ArgumentExpressions.GetAllRegisterTokenPoolInfo(owner);
		}
		return null;
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}
}

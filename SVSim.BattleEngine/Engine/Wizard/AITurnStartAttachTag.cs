using System.Collections.Generic;

namespace Wizard;

public class AITurnStartAttachTag : AITurnStartTagArgument
{
	private AIPolishConvertedExpression _selectCountArg;

	public AIScriptTokenArgType RemoveTiming { get; private set; }

	public AIPlayTag Tag { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 4;

	public AITurnStartAttachTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		base.InitExpressions(list[0]);
		Tag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[1], list[2], list[3]);
		RemoveTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2]);
		_selectCountArg = _exprList[_exprList.Count - 3];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AIAttachTagSimulationUtility.SimulateAttachTagToAll(targetsFromField, tagOwner, Tag, RemoveTiming, situation);
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}

	public override AITokenIdCollection GetAllRegisterTokenPoolInfo(AIVirtualCard owner)
	{
		if (Tag != null)
		{
			return Tag.ArgumentExpressions.GetAllRegisterTokenPoolInfo(owner);
		}
		return null;
	}
}

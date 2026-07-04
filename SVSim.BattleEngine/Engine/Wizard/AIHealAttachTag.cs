using System.Collections.Generic;

namespace Wizard;

public class AIHealAttachTag : AITriggerAndTargetFiltersTagBase
{
	private AIPolishConvertedExpression _selectCountArg;

	public AIPlayTag Tag { get; private set; }

	public AIScriptTokenArgType SelectType { get; private set; }

	public AIScriptTokenArgType RemoveTiming { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 3;

	public AIHealAttachTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		base.InitExpressions(list[0]);
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 3], base.LegalSelectTypes);
		_selectCountArg = _exprList[_exprList.Count - 2];
		RemoveTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 1]);
		if (list.Count <= AIPlayTag.TAG_WORDS_LENTGH)
		{
			Tag = null;
		}
		else
		{
			Tag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[1], list[2], list[3]);
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[3]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.RANDOM_MULTI_SELECT
		};
	}

	private int GetSelectCount(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (_selectCountArg == null)
		{
			return 0;
		}
		return (int)_selectCountArg.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			switch (SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIAttachTagSimulationUtility.SimulateAttachTagToAll(targets, tagOwner, Tag, RemoveTiming, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
			{
				int selectCount = GetSelectCount(tagOwner, playPtn, situation);
				AIDefaultSelectLogicArgument selectLogic = new AIDefaultSelectLogicArgument(null);
				AIAttachTagSimulationUtility.SimulateRandomSelectAttachTag(targets, selectCount, tagOwner, field, playPtn, situation, Tag, RemoveTiming, selectLogic);
				break;
			}
			}
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

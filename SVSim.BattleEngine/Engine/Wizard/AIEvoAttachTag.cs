using System.Collections.Generic;

namespace Wizard;

public class AIEvoAttachTag : AIEvoTagArgument
{
	private AIScriptTokenArgType _removeTiming;

	private AISelectLogicArgumentBase _selectLogicArg;

	private readonly int REMOVE_TIMING_OFFSET = 1;

	public AIPlayTag Tag { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIEvoAttachTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		base.InitExpressions(list[0]);
		_removeTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - REMOVE_TIMING_OFFSET]);
		Tag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[1], list[2], list[3]);
		if (list.Count > 4)
		{
			_selectLogicArg = AISelectLogicSimulationUtility.CreateSelectLogicArgument(list[4]);
		}
		else
		{
			_selectLogicArg = new AIDefaultSelectLogicArgument(null);
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return;
		}
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			AIAttachTagSimulationUtility.SimulateAttachTagToAll(targetsFromField, tagOwner, Tag, _removeTiming, situation);
			break;
		case AIScriptTokenArgType.TARGET_SELECT:
		case AIScriptTokenArgType.SECOND_TARGET_SELECT:
			if (situation.IsTargetExists(base.SelectType))
			{
				AIAttachTagSimulationUtility.SimulateAttachTagToTarget(situation, tagOwner, base.SelectType, Tag, _removeTiming);
			}
			else
			{
				AIAttachTagSimulationUtility.SimulateAttachTagToSingle(_selectLogicArg.SelectSingleTarget(targetsFromField, tagOwner, field, playPtn, situation, AISelectTargetPattern.Best), tagOwner, Tag, _removeTiming, situation);
			}
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
		{
			AIDefaultSelectLogicArgument selectLogic = new AIDefaultSelectLogicArgument(null);
			AIAttachTagSimulationUtility.SimulateRandomSelectAttachTag(targetsFromField, 1, tagOwner, field, playPtn, situation, Tag, _removeTiming, selectLogic);
			break;
		}
		case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
			break;
		}
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[4]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT
		};
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

using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayAttachTag : AIWhenPlayTagArgument
{
	private AISelectLogicArgumentBase _selectLogicArg;

	public AIPlayTag Tag { get; private set; }

	public AIScriptTokenArgType RemoveTiming { get; private set; }

	protected override bool _isSelectCountImplemented => true;

	protected override int SELECT_TYPE_OFFSET => 3;

	protected override int SELECT_COUNT_OFFSET => 2;

	public AIWhenPlayAttachTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		base.InitExpressions(list[0]);
		RemoveTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 1]);
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
			AIAttachTagSimulationUtility.SimulateAttachTagToAll(targetsFromField, tagOwner, Tag, RemoveTiming, situation);
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
		case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
		{
			int selectCount = GetSelectCount(tagOwner, field, playPtn, situation);
			if (selectCount > 0)
			{
				AIAttachTagSimulationUtility.SimulateRandomSelectAttachTag(targetsFromField, selectCount, tagOwner, field, playPtn, situation, Tag, RemoveTiming, _selectLogicArg);
			}
			break;
		}
		case AIScriptTokenArgType.TARGET_SELECT:
		case AIScriptTokenArgType.SECOND_TARGET_SELECT:
		{
			if (situation.IsTargetExists(base.SelectType))
			{
				AIAttachTagSimulationUtility.SimulateAttachTagToTarget(situation, tagOwner, base.SelectType, Tag, RemoveTiming);
				break;
			}
			int selectCount = GetSelectCount(tagOwner, field, playPtn, situation);
			if (selectCount > 0)
			{
				SimulateTargetSelectAttachTag(targetsFromField, selectCount, tagOwner, field, playPtn, situation);
			}
			break;
		}
		}
	}

	private void SimulateTargetSelectAttachTag(List<AIVirtualCard> candidates, int selectCount, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return;
		}
		if (selectCount == candidates.Count)
		{
			AIAttachTagSimulationUtility.SimulateAttachTagToAll(candidates, tagOwner, Tag, RemoveTiming, situation);
		}
		else
		{
			if (selectCount > candidates.Count)
			{
				return;
			}
			if (selectCount == 1)
			{
				AIAttachTagSimulationUtility.SimulateAttachTagToSingle(_selectLogicArg.SelectSingleTarget(candidates, tagOwner, field, playPtn, situation, AISelectTargetPattern.Best), tagOwner, Tag, RemoveTiming, situation);
				return;
			}
			List<AIVirtualCard> list = _selectLogicArg.SelectMultipleSelectedTargets(candidates, selectCount, tagOwner, field, playPtn, situation, AISelectTargetPattern.Best);
			if (list != null && list.Count > 0)
			{
				AIAttachTagSimulationUtility.SimulateAttachTagToAll(list, tagOwner, Tag, RemoveTiming, situation);
			}
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[5]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.RANDOM_MULTI_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT
		};
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		list.AddRange(field.CardListSet.AllReferableCards);
		list.AddRange(field.GetEnemyHandCardList());
		return list;
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

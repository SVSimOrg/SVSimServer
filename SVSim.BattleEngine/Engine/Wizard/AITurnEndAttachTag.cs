using System.Collections.Generic;

namespace Wizard;

public class AITurnEndAttachTag : AIFiltersArgument, IAITurnEndArgument
{
	private AIPolishConvertedExpression AttachCount;

	public AIPlayTag Tag { get; private set; }

	public AIScriptTokenArgType SelectType { get; private set; }

	public AIScriptTokenArgType RemoveTiming { get; private set; }

	public bool IsAllyTurn { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 4;

	public AITurnEndAttachTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		base.InitExpressions(list[0]);
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 4], base.LegalSelectTypes);
		AttachCount = _exprList[_exprList.Count - 3];
		RemoveTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2]);
		IsAllyTurn = TurnEndTagCollection.IsAllyTurn(_exprList, GetType(), _exprList.Count - 1);
		if (list.Count <= AIPlayTag.TAG_WORDS_LENTGH)
		{
			Tag = null;
		}
		else
		{
			Tag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[1], list[2], list[3]);
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIAttachTagSimulationUtility.SimulateAttachTagToAll(targetsFromField, tagOwner, Tag, RemoveTiming, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
			{
				int selectCount = (int)AttachCount.EvalArg(tagOwner, playPtn, tagOwner.SelfField);
				AIDefaultSelectLogicArgument selectLogic = new AIDefaultSelectLogicArgument(null);
				AIAttachTagSimulationUtility.SimulateRandomSelectAttachTag(targetsFromField, selectCount, tagOwner, field, playPtn, situation, Tag, RemoveTiming, selectLogic);
				break;
			}
			}
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT
		};
	}

	public float CalculateThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allInplayStatusList)
	{
		return 0f;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}
}

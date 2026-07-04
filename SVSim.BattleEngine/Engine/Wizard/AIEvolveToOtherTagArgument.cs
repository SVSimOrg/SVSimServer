using System.Collections.Generic;

namespace Wizard;

public class AIEvolveToOtherTagArgument : AIScriptArgumentExpressions
{
	private AIVirtualCard _evolveCandidate;

	public AIEvolveToOtherTagArgument(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		if (_exprList.Count <= 0)
		{
			AIConsoleUtility.LogError("AIEvolveToOtherTagArgument error!! argments is empty !!");
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		AIVirtualCard evolveCandidates = GetEvolveCandidates(tagOwner, field, playPtn, situation);
		if (evolveCandidates == null)
		{
			AIConsoleUtility.LogError("AIEvolveToOtherTagArgument error!! evolve target card not found!");
			return;
		}
		tagOwner.CreateOtherEvolveParameterFromBattleCardBase(field.AI, evolveCandidates.BaseCard, tagOwner.BaseCard, situation);
		field.CardListSet.TagClassification(tagOwner);
	}

	private AIVirtualCard GetEvolveCandidates(AIVirtualCard parent, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (_evolveCandidate == null)
		{
			int evolveCandidateId = GetEvolveCandidateId(parent, field, playPtn, situation);
			if (evolveCandidateId < 10000)
			{
				AIConsoleUtility.LogError("AIEvolveToOtherTagArgument.GetEvolveCandidates() error!! cant get card id!");
				return null;
			}
			AIVirtualCard tokenFromId = field.AI.tokenManager.GetTokenFromId(evolveCandidateId, parent.IsAlly, field);
			_evolveCandidate = tokenFromId;
		}
		return _evolveCandidate;
	}

	private int GetEvolveCandidateId(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		int result = -1;
		AITokenIdCollection bothSideTokenIdListFromFilter = AISummonTokenUtility.GetBothSideTokenIdListFromFilter(tagOwner, field, null, _exprList[0].TokenList, AITokenType.Default, AIScriptTokenArgType.ALLY, AIScriptTokenArgType.ALL_SELECT, 1, playPtn, situation);
		if (bothSideTokenIdListFromFilter != null)
		{
			List<AITokenInformation> list = (tagOwner.IsAlly ? bothSideTokenIdListFromFilter.AllyTokenIdList : bothSideTokenIdListFromFilter.OpponentTokenIdList);
			if (list == null || list.Count <= 0)
			{
				return result;
			}
			result = list[0].TokenId;
			if (list.Count > 1)
			{
				AIConsoleUtility.LogWarning("GetEvolveCandidateId(): Multiple evolve candidate id found! Check card argments! [CardName:" + tagOwner.CardName + "]");
			}
		}
		return result;
	}
}

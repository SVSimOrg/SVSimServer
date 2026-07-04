using System.Collections.Generic;

namespace Wizard;

public abstract class AIWhenPlayTokenArgumentBase : AIWhenPlayTagArgument
{
	private AITokenType _tokenType;

	protected AITokenIdHolderCandidateRangeInformation _candidateRangeInfo;

	protected int _ommittedIndexOffset;

	protected int _realNonFilterFirstOffset;

	public virtual AIScriptTokenArgType SideType { get; protected set; }

	protected override int SELECT_TYPE_OFFSET => -1;

	protected override int NON_FILTER_FIRST_OFFSET => _realNonFilterFirstOffset;

	public AIWhenPlayTokenArgumentBase(string text, AITokenType tokenType)
		: base(text)
	{
		_tokenType = tokenType;
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		InitializeSide();
		InitializeSelectCount();
		InitSelectType();
		InitializeFilter();
	}

	protected virtual void InitializeSide()
	{
		if (AIPlayTagInitializingUtility.TryCreateTokenSideType(_exprList[_exprList.Count - 1], out var sideType))
		{
			_ommittedIndexOffset = 0;
			SideType = sideType;
		}
		else
		{
			_ommittedIndexOffset = 1;
			SideType = AIScriptTokenArgType.ALLY;
		}
	}

	protected override void InitializeFilter()
	{
		base.InitializeFilter();
		_candidateRangeInfo = CreateCandidateRangeInfo();
	}

	protected virtual AITokenIdHolderCandidateRangeInformation CreateCandidateRangeInfo()
	{
		return new AITokenIdHolderCandidateRangeInformation(base.Filters);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		AITokenIdCollection bothSideTokenIdCollection = GetBothSideTokenIdCollection(tagOwner, field, playPtn, situation);
		if (bothSideTokenIdCollection != null && bothSideTokenIdCollection.HasToken)
		{
			bothSideTokenIdCollection.SummonAllTokenToField(field, tagOwner, situation);
		}
	}

	public override void PseudoExecute(AIVirtualField field, AISinglePlayptnRecord record, PlayedCardInfo playInfo, AIVirtualTargetSelectAction situation)
	{
		if (record == null || record.FirstSummonedAllyFollower != null || base.SelectType == AIScriptTokenArgType.RANDOM_SELECT || base.SelectType == AIScriptTokenArgType.RANDOM_MULTI_SELECT)
		{
			return;
		}
		AIVirtualCard card = playInfo.Card;
		List<int> playPtn = record.PlayPtn;
		List<AITokenInformation> allyTokenIdList = GetAllyTokenIdList(card, field, playPtn, situation);
		if (allyTokenIdList != null && allyTokenIdList.Count > 0)
		{
			AIVirtualCard aIVirtualCard = AISummonTokenUtility.FirstSummonedFollowerTokenCandidate(allyTokenIdList, card, field, situation, isTokenAlly: true, isSkillSummon: true);
			if (aIVirtualCard != null)
			{
				record.FirstSummonedAllyFollower = aIVirtualCard;
			}
		}
	}

	public virtual AITokenIdCollection GetBothSideTokenIdCollection(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (situation.IsLatestAction && (base.SelectType == AIScriptTokenArgType.RANDOM_SELECT || base.SelectType == AIScriptTokenArgType.RANDOM_MULTI_SELECT))
		{
			return GetBothSideTokenIdCollectionFromRealTargetInformation(tagOwner, field, playPtn, situation);
		}
		return GetBothSideTokenIdCollectionFromTag(tagOwner, field, playPtn, situation);
	}

	protected virtual AITokenIdCollection GetBothSideTokenIdCollectionFromRealTargetInformation(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		bool isTokenAlly = AISummonTokenUtility.GetIsTokenAlly(tagOwner, SideType);
		List<int> tokenIdListFromDequeuedRealTargetInfo = situation.GetTokenIdListFromDequeuedRealTargetInfo(tagOwner, _tokenType);
		if (tokenIdListFromDequeuedRealTargetInfo == null || tokenIdListFromDequeuedRealTargetInfo.Count <= 0)
		{
			return null;
		}
		return AISummonTokenUtility.CreateTokenIdCollection(tagOwner, tokenIdListFromDequeuedRealTargetInfo, isTokenAlly, _tokenType);
	}

	protected abstract AITokenIdCollection GetBothSideTokenIdCollectionFromTag(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation);

	public abstract List<AITokenInformation> GetAllyTokenIdList(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation);

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return _candidateRangeInfo.GetTokenIdHolderRange(field);
	}
}

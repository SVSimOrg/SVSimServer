using System.Collections.Generic;

namespace Wizard;

public class PlaySkipWithActionInformation : PlaySkipInformation
{
	public AIScriptTokenArgType Action;

	public PlaySkipWithActionInformation(AIScriptTokenArgType action)
	{
		TagType = AIPlayTagType.PlaySkipWithAction;
		Action = action;
		base.IsEvolutionPermittedTag = false;
	}

	public AIVirtualActionInfo GetExtraActionBase(AISinglePlayptnRecord playPtnRecord, ref List<AIScriptTokenArgType> checkedActionTypeList)
	{
		if (checkedActionTypeList != null && checkedActionTypeList.Contains(Action))
		{
			return null;
		}
		AIVirtualActionInfo result = ((Action != AIScriptTokenArgType.NEXT_PLAY) ? null : GetNextPlayActionBase(playPtnRecord));
		checkedActionTypeList = AIParamQuery.AddElementToList(Action, checkedActionTypeList);
		return result;
	}

	private AIVirtualActionInfo GetNextPlayActionBase(AISinglePlayptnRecord record)
	{
		if (record.PlayedCardList.Count <= 0)
		{
			return null;
		}
		PlayedCardInfo playedCardInfo = record.PlayedCardList[0];
		if (playedCardInfo.TransformCard == null)
		{
			return new AIVirtualTargetSelectAction(playedCardInfo.Card, playedCardInfo.Card, AIOperationType.PLAY);
		}
		return new AIVirtualTargetSelectAction(playedCardInfo.TransformCard, playedCardInfo.Card, AIOperationType.PLAY);
	}
}

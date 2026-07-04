using System.Collections.Generic;

namespace Wizard;

public class PlaySkipWithActionIfEvoInformation : PlaySkipInformation
{
	private readonly PlaySkipIfEvoInformation _playSkipIfEvo;

	private readonly PlaySkipWithActionInformation _playSkipWithAction;

	public PlaySkipWithActionIfEvoInformation(AIScriptTokenArgType action)
	{
		TagType = AIPlayTagType.PlaySkipWithActionIfEvo;
		base.IsEvolutionPermittedTag = true;
		_playSkipIfEvo = new PlaySkipIfEvoInformation();
		_playSkipWithAction = new PlaySkipWithActionInformation(action);
	}

	public override bool IsEvoCardLegal(AIVirtualCard evoCard)
	{
		return _playSkipIfEvo.IsEvoCardLegal(evoCard);
	}

	public void AddEvolutionPermittedCards(List<AIVirtualCard> cards)
	{
		_playSkipIfEvo.AddEvolutionPermittedCards(cards);
	}

	public AIVirtualActionInfo GetExtraActionBase(AISinglePlayptnRecord playPtnRecord, ref List<AIScriptTokenArgType> checkedActionTypeList)
	{
		return _playSkipWithAction.GetExtraActionBase(playPtnRecord, ref checkedActionTypeList);
	}
}

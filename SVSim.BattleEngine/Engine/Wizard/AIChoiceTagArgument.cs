using System.Collections.Generic;

namespace Wizard;

public class AIChoiceTagArgument : AIScriptArgumentExpressions
{
	protected List<int> _choiceIds;

	protected AIPolishConvertedExpression _choiceCount;

	private List<AIVirtualCard> _playerChoiceFakeCards;

	private List<AIVirtualCard> _enemyChoiceFakeCards;

	public AIChoiceTagArgument(string text)
		: base(text)
	{
		_playerChoiceFakeCards = null;
		_enemyChoiceFakeCards = null;
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_choiceCount = _exprList[_exprList.Count - 1];
		_choiceIds = new List<int>();
		for (int i = 0; i < _exprList.Count - 1; i++)
		{
			_choiceIds.Add(_exprList[i].EvalID());
		}
	}

	public virtual List<AIVirtualCard> GetChoiceTargets(AIVirtualCard owner, AIVirtualField field)
	{
		if (owner.IsPlayer && _playerChoiceFakeCards == null)
		{
			CreateChoiceTargets(owner, field, out _playerChoiceFakeCards);
		}
		else if (!owner.IsPlayer && _enemyChoiceFakeCards == null)
		{
			CreateChoiceTargets(owner, field, out _enemyChoiceFakeCards);
		}
		if (!owner.IsPlayer)
		{
			return _enemyChoiceFakeCards;
		}
		return _playerChoiceFakeCards;
	}

	private void CreateChoiceTargets(AIVirtualCard owner, AIVirtualField field, out List<AIVirtualCard> choiceFakeCards)
	{
		choiceFakeCards = new List<AIVirtualCard>();
		AITokenManager tokenManager = field.AI.tokenManager;
		for (int i = 0; i < _choiceIds.Count; i++)
		{
			AIVirtualCard choiceTokenFromId = tokenManager.GetChoiceTokenFromId(_choiceIds[i], owner.IsAlly);
			if (choiceTokenFromId == null)
			{
				AIConsoleUtility.LogError("GetChoiceTarget error!! choiceBaseCard is null");
				break;
			}
			ChoiceVirtualCard choiceVirtualCard = new ChoiceVirtualCard(choiceTokenFromId.BaseCard, owner.IsAlly, field);
			choiceVirtualCard.InitializeTags(field.ParamQuery, null, null);
			choiceFakeCards.Add(choiceVirtualCard);
		}
	}

	public virtual int GetChoiceCount(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation)
	{
		return (int)_choiceCount.EvalArg(owner, null, field, situation);
	}

	protected override AITokenIdCollection CreateRegisterTokenPoolInfo(AIVirtualCard owner, List<int> idList)
	{
		return AISummonTokenUtility.CreateTokenIdCollectionFromIdList(owner, AIScriptTokenArgType.ALLY, idList, AITokenType.Choice);
	}
}

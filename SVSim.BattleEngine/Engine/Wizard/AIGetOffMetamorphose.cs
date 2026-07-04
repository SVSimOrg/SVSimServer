using System.Collections.Generic;

namespace Wizard;

public class AIGetOffMetamorphose : AIFiltersAndSelectTypeArgument
{
	private AIPolishConvertedExpression _tokenId;

	private AIScriptTokenArgType _registerSide = AIScriptTokenArgType.BOTH;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIGetOffMetamorphose(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_tokenId = _exprList[_exprList.Count - 1];
		if (IsSideTokenArgType(_exprList[0], out var dstTokenARgType))
		{
			_registerSide = dstTokenARgType;
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		int tokenId = GetTokenId();
		if (tokenId != -1)
		{
			AIMetamorphoseSimulationUtility.MetamorphoseAll(field, targetsFromField, tokenId, tagOwner, situation);
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	public int GetTokenId()
	{
		if (_tokenId == null)
		{
			AIConsoleUtility.LogError("AIGetOffMetamorphose error!! _tokenId is null");
			return -1;
		}
		return _tokenId.EvalID();
	}

	protected override AITokenIdCollection CreateRegisterTokenPoolInfo(AIVirtualCard owner, List<int> idList)
	{
		return AISummonTokenUtility.CreateTokenIdCollectionFromIdList(owner, _registerSide, idList, AITokenType.Default);
	}
}

using System.Collections.Generic;

namespace Wizard;

public class AIEvoMetamorphose : AIEvoTagArgument
{
	private AIPolishConvertedExpression _metamorphoseId;

	private readonly int METAMORPHOSE_ID_ARG_OFFSET = 2;

	private AIScriptTokenArgType _registerSide = AIScriptTokenArgType.BOTH;

	protected override bool _isSelectCountImplemented => true;

	protected override int SELECT_TYPE_OFFSET => 3;

	public AIEvoMetamorphose(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_metamorphoseId = _exprList[_exprList.Count - METAMORPHOSE_ID_ARG_OFFSET];
		if (IsSideTokenArgType(_exprList[0], out var dstTokenARgType))
		{
			_registerSide = dstTokenARgType;
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int metamorphoseId = _metamorphoseId.EvalID();
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIMetamorphoseSimulationUtility.MetamorphoseAll(field, targetsFromField, metamorphoseId, tagOwner, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AIMetamorphoseSimulationUtility.MetamorphoseRandom(field, targetsFromField, metamorphoseId, tagOwner, playPtn, situation);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				AIMetamorphoseSimulationUtility.MetamorphoseTarget(field, targetsFromField, metamorphoseId, playPtn, situation, base.SelectType, GetSelectCount(tagOwner, field, playPtn, situation));
				break;
			case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
				break;
			}
		}
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	public override AIRemovalType GetRemovalType()
	{
		return AIRemovalType.Metamorphose;
	}

	protected override AITokenIdCollection CreateRegisterTokenPoolInfo(AIVirtualCard owner, List<int> idList)
	{
		return AISummonTokenUtility.CreateTokenIdCollectionFromIdList(owner, _registerSide, idList, AITokenType.Default);
	}
}

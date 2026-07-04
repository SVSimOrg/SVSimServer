using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayMetamorphose : AIWhenPlayTagArgument
{
	private readonly int METAMORPHOSE_ID_ARG_OFFSET = 1;

	private AIScriptTokenArgType _registerSide = AIScriptTokenArgType.BOTH;

	public AIPolishConvertedExpression MetamorphoseId { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIWhenPlayMetamorphose(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		MetamorphoseId = _exprList[_exprList.Count - METAMORPHOSE_ID_ARG_OFFSET];
		if (IsSideTokenArgType(_exprList[0], out var dstTokenARgType))
		{
			_registerSide = dstTokenARgType;
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[4]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT
		};
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int metamorphoseId = MetamorphoseId.EvalID();
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				UpdateSituationRemovalType(situation);
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

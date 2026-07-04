using System.Collections.Generic;

namespace Wizard;

public class AILastwordMetamorphose : AIFiltersAndSelectTypeArgument
{
	private readonly int METAMORPHOSE_ID_ARG_OFFSET = 1;

	public AIScriptTokenArgType SideType { get; protected set; }

	public AIPolishConvertedExpression MetamorphoseID { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AILastwordMetamorphose(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		MetamorphoseID = _exprList[_exprList.Count - METAMORPHOSE_ID_ARG_OFFSET];
		AIPolishConvertedExpression arg = _exprList[0];
		AIScriptTokenArgType dstTokenARgType = AIScriptTokenArgType.NONE;
		if (IsSideTokenArgType(arg, out dstTokenARgType))
		{
			SideType = dstTokenARgType;
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int metamorphoseId = MetamorphoseID.EvalID();
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIMetamorphoseSimulationUtility.MetamorphoseAll(field, targetsFromField, metamorphoseId, tagOwner, situation);
			}
			else if (base.SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				AIMetamorphoseSimulationUtility.MetamorphoseRandom(field, targetsFromField, metamorphoseId, tagOwner, playPtn, situation);
			}
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	protected override AITokenIdCollection CreateRegisterTokenPoolInfo(AIVirtualCard owner, List<int> idList)
	{
		return AISummonTokenUtility.CreateTokenIdCollectionFromIdList(owner, SideType, idList, AITokenType.Default);
	}
}

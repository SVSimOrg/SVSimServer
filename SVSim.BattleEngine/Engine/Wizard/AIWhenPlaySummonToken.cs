using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlaySummonToken : AIWhenPlayTokenArgumentBase
{
	protected override bool _isSelectCountImplemented => true;

	protected override int SELECT_COUNT_OFFSET => 2;

	protected override int SELECT_TYPE_OFFSET => 3;

	public AIWhenPlaySummonToken(string text)
		: base(text, AITokenType.Default)
	{
	}

	protected override AITokenIdCollection GetBothSideTokenIdCollectionFromTag(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation, isBlockDead: false);
		int selectCount = GetSelectCount(tagOwner, field, playPtn, situation);
		return AISummonTokenUtility.GetBothSideTokenIdListFromFilter(tagOwner, field, targetsFromField, base.Filters, AITokenType.Default, SideType, base.SelectType, selectCount, playPtn, situation);
	}

	public override List<AITokenInformation> GetAllyTokenIdList(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		AITokenIdCollection bothSideTokenIdCollection = GetBothSideTokenIdCollection(tagOwner, field, playPtn, situation);
		if (bothSideTokenIdCollection == null || !bothSideTokenIdCollection.HasAllyToken)
		{
			return null;
		}
		return bothSideTokenIdCollection.AllyTokenIdList;
	}

	protected override void InitializeSelectCount()
	{
		int num = (_expectedSelectCountArgOffset = SELECT_COUNT_OFFSET - _ommittedIndexOffset);
		AIPolishConvertedExpression aIPolishConvertedExpression = _exprList[_exprList.Count - num];
		if (aIPolishConvertedExpression.IsMathematicExpress())
		{
			_selectCount = aIPolishConvertedExpression;
			return;
		}
		AIConsoleUtility.LogError("AIWhenPlaySummonToken.InitializeSelectCount error!! Cannot find _selectCount from argument expression!!!!!");
		_ommittedIndexOffset++;
	}

	protected override void InitSelectType()
	{
		int num = SELECT_TYPE_OFFSET - _ommittedIndexOffset;
		if (AIPlayTagInitializingUtility.TryCreateSelectType(_exprList[_exprList.Count - num], base.LegalSelectTypes, out var selectType))
		{
			base.SelectType = selectType;
		}
		else
		{
			_ommittedIndexOffset++;
			base.SelectType = AIScriptTokenArgType.ALL_SELECT;
		}
		_realNonFilterFirstOffset = SELECT_TYPE_OFFSET - _ommittedIndexOffset;
	}

	public override List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return GetCandidateRange(field);
	}
}

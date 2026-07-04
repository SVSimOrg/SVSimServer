using System;
using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayCopyTag : AIWhenPlayTagArgument
{
	private List<AIScriptTokenArgType> _timingList;

	private List<AIScriptTokenBase> _receiverFilterList;

	protected override int SELECT_TYPE_OFFSET => _timingList.Count + 1;

	public AIWhenPlayCopyTag(string text)
		: base(text)
	{
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.TARGET_SELECT
		};
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		InitTimingListAndSelectType();
		InitializeFilter();
		_receiverFilterList = new List<AIScriptTokenBase>();
		AIScriptArgumentToken item = new AIScriptArgumentToken(AIScriptTokenArgType.SELF, isNot: false);
		_receiverFilterList.Add(item);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AICopyTagSimulationUtility.ExecuteCopyAndAttachTagToAll(tagOwner, targetsFromField, _timingList, field, playPtn, situation);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
				AICopyTagSimulationUtility.ExecuteCopyAndAttachTagToSelectedTarget(tagOwner, targetsFromField, _timingList, base.SelectType, field, playPtn, situation);
				break;
			}
		}
	}

	private void InitTimingListAndSelectType()
	{
		_timingList = new List<AIScriptTokenArgType>();
		for (int num = _exprList.Count - 1; num >= 0; num--)
		{
			AIPolishConvertedExpression aIPolishConvertedExpression = _exprList[num];
			if (aIPolishConvertedExpression.TokenList == null || aIPolishConvertedExpression.TokenList.Count <= 0)
			{
				break;
			}
			if (aIPolishConvertedExpression.TokenList[0] is AIScriptArgumentToken { ArgumentType: var argumentType })
			{
				if (Array.IndexOf(base.LegalSelectTypes, argumentType) >= 0)
				{
					base.SelectType = argumentType;
					break;
				}
				_timingList.Add(argumentType);
			}
		}
	}
}

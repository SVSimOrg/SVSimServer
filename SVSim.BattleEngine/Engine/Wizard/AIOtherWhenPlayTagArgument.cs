using System;
using System.Collections.Generic;

namespace Wizard;

public abstract class AIOtherWhenPlayTagArgument : AITriggerAndTargetFiltersTagBase
{
	public virtual AIScriptTokenArgType SelectType { get; protected set; }

	public PlaySimulationType RequiredPlayType { get; protected set; }

	protected virtual int SELECT_TYPE_OFFSET => 1;

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_OFFSET;

	public AIOtherWhenPlayTagArgument(string text, PlaySimulationType playType = PlaySimulationType.Undefined)
		: base(text)
	{
		RequiredPlayType = playType;
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (IsLegalSelectType(_exprList[_exprList.Count - SELECT_TYPE_OFFSET], out var selectType))
		{
			SelectType = selectType;
		}
		else
		{
			AIConsoleUtility.LogError("AIOtherWhenPlayTagArgument Error!!! SelectType is " + selectType);
		}
	}

	public virtual void TargetLifePrediction(AIVirtualCard target, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, LifeRecord targetLifeRecord)
	{
		throw new NotImplementedException();
	}

	public virtual void MultipleTargetLifePrediction(List<AIVirtualCard> targetList, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<LifeRecord> lifeList)
	{
		throw new NotImplementedException();
	}
}

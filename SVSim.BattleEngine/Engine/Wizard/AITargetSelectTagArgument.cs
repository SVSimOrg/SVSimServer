using System.Collections.Generic;

namespace Wizard;

public abstract class AITargetSelectTagArgument : AIFiltersAndSelectTypeArgument
{
	protected AIPolishConvertedExpression _selectCount;

	protected int _expectedSelectCountArgOffset;

	protected virtual bool _isSelectCountImplemented => false;

	public bool IsForbiddenSelectedTarget { get; private set; }

	protected virtual int SELECT_COUNT_OFFSET => 1;

	protected override int SELECT_TYPE_OFFSET => _expectedSelectCountArgOffset + 1;

	public AITargetSelectTagArgument(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		InitializeSelectCount();
		InitSelectType();
		InitializeFilter();
		SetUpReferenceSelectedTargetInfoSelectType();
		int forbiddenSelectedTargetFilterIndex = AITargetSelectFilteringUtility.GetForbiddenSelectedTargetFilterIndex(base.Filters);
		if (forbiddenSelectedTargetFilterIndex >= 0)
		{
			IsForbiddenSelectedTarget = true;
			base.Filters.RemoveAt(forbiddenSelectedTargetFilterIndex);
		}
		else
		{
			IsForbiddenSelectedTarget = false;
		}
	}

	protected virtual void InitializeSelectCount()
	{
		_selectCount = null;
		if (_isSelectCountImplemented)
		{
			_expectedSelectCountArgOffset = SELECT_COUNT_OFFSET;
			AIPolishConvertedExpression aIPolishConvertedExpression = _exprList[_exprList.Count - _expectedSelectCountArgOffset];
			if (aIPolishConvertedExpression.IsMathematicExpress())
			{
				_selectCount = aIPolishConvertedExpression;
			}
		}
		if (_selectCount == null)
		{
			_expectedSelectCountArgOffset = SELECT_COUNT_OFFSET - 1;
		}
	}

	public int GetSelectCount(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_selectCount == null)
		{
			return 1;
		}
		return (int)_selectCount.EvalArg(owner, playPtn, field, situation);
	}
}

using System.Collections.Generic;

namespace Wizard;

public class AIBonusArgumentWithIgnoreInBattle : AIScriptArgumentExpressions
{
	protected AIPolishConvertedExpression _bonusValueArg;

	protected int _valueIndexOffset;

	public bool IsIgnoreInBattle { get; private set; }

	public AIBonusArgumentWithIgnoreInBattle(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (!(_exprList[_exprList.Count - 1].TokenList[0] is AIScriptArgumentToken aIScriptArgumentToken) || (aIScriptArgumentToken.ArgumentType != AIScriptTokenArgType.IGNORE_IN_BATTLE && aIScriptArgumentToken.ArgumentType != AIScriptTokenArgType.IGNORE_IN_FUSION))
		{
			_valueIndexOffset = 1;
			IsIgnoreInBattle = false;
		}
		else
		{
			_valueIndexOffset = 2;
			IsIgnoreInBattle = true;
		}
		_bonusValueArg = _exprList[_exprList.Count - _valueIndexOffset];
	}

	public virtual float GetBonusValue(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool useIgnoreInBattle)
	{
		if (tagOwner == null || (useIgnoreInBattle && IsIgnoreInBattle))
		{
			return 0f;
		}
		return _bonusValueArg.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
	}
}

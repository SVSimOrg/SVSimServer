using System.Linq;

namespace Wizard;

public class AIRemoveSkill : AIScriptArgumentExpressions
{
	private readonly AIScriptTokenArgType[] LEGAL_TIMING_ARGS = new AIScriptTokenArgType[2]
	{
		AIScriptTokenArgType.WHEN_DESTROY,
		AIScriptTokenArgType.WHEN_CLASH
	};

	private readonly AIScriptTokenArgType[] LEGAL_TYPE_ARGS = new AIScriptTokenArgType[2]
	{
		AIScriptTokenArgType.ALL,
		AIScriptTokenArgType.KILLER
	};

	public AIScriptTokenArgType Timing { get; private set; }

	public AIScriptTokenArgType RemoveSkillType { get; private set; }

	public AIRemoveSkill(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		Timing = AIScriptTokenArgType.NONE;
		RemoveSkillType = AIScriptTokenArgType.NONE;
		int num = -1;
		AIScriptTokenArgType legalType = AIScriptTokenArgType.NONE;
		for (int i = 0; i < _exprList.Count; i++)
		{
			if (IsLegalType(_exprList[i], LEGAL_TYPE_ARGS, out legalType))
			{
				num = i;
				RemoveSkillType = legalType;
				break;
			}
		}
		if (num < 0)
		{
			return;
		}
		int num2 = -1;
		for (int j = 0; j < _exprList.Count; j++)
		{
			if (IsLegalType(_exprList[j], LEGAL_TIMING_ARGS, out legalType))
			{
				num2 = j;
				Timing = legalType;
				break;
			}
		}
		_ = 0;
	}

	private bool IsLegalType(AIPolishConvertedExpression arg, AIScriptTokenArgType[] argArray, out AIScriptTokenArgType legalType)
	{
		legalType = AIScriptTokenArgType.NONE;
		if (arg.TokenList != null && arg.TokenList.Count > 0)
		{
			AIScriptTokenBase aIScriptTokenBase = arg.TokenList[0];
			if (aIScriptTokenBase is AIScriptArgumentToken)
			{
				AIScriptTokenArgType argumentType = ((AIScriptArgumentToken)aIScriptTokenBase).ArgumentType;
				if (argArray.Contains(argumentType))
				{
					legalType = argumentType;
					return true;
				}
			}
		}
		return false;
	}
}

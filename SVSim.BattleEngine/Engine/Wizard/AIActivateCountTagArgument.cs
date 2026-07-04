namespace Wizard;

public class AIActivateCountTagArgument : AIScriptArgumentExpressions
{
	private static readonly AIScriptTokenArgType[] LegalResetSideTypes = new AIScriptTokenArgType[4]
	{
		AIScriptTokenArgType.ALLY,
		AIScriptTokenArgType.OPPONENT,
		AIScriptTokenArgType.BOTH,
		AIScriptTokenArgType.NONE
	};

	public AIScriptTokenArgType ResetSideType { get; protected set; }

	public int SkillOwnerId { get; protected set; }

	public int SkillIndex { get; protected set; }

	public int TurnMaxActivateCount { get; protected set; }

	public AIActivateCountTagArgument(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		TurnMaxActivateCount = -1;
		if (_exprList[_exprList.Count - 1].TokenList[0] is AIScriptNumericToken aIScriptNumericToken)
		{
			SkillIndex = (int)aIScriptNumericToken.Value;
			ResetSideType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 3], LegalResetSideTypes);
			if (_exprList[_exprList.Count - 2].TokenList[0] is AIScriptIDToken aIScriptIDToken)
			{
				SkillOwnerId = aIScriptIDToken.ID;
			}
			else
			{
				SkillOwnerId = -1;
			}
		}
		else
		{
			SkillIndex = 0;
			SkillOwnerId = -1;
		}
	}
}

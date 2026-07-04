namespace Wizard;

public class AITurnStartTagArgument : AIFiltersAndSelectTypeArgument
{
	private AIScriptTokenArgType _sideType;

	private readonly int SIDETYPE_OFFSET = 1;

	private readonly AIScriptTokenArgType[] _legalSideTypes = new AIScriptTokenArgType[3]
	{
		AIScriptTokenArgType.BOTH,
		AIScriptTokenArgType.ALLY,
		AIScriptTokenArgType.OPPONENT
	};

	protected override int SELECT_TYPE_OFFSET => 2;

	public AITurnStartTagArgument(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		InitSideType();
	}

	protected virtual void InitSideType()
	{
		_sideType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SIDETYPE_OFFSET], _legalSideTypes);
	}

	public bool CheckTurnStartSide(AIScriptTokenArgType turnStartSide)
	{
		if (_sideType == AIScriptTokenArgType.BOTH)
		{
			return true;
		}
		return _sideType == turnStartSide;
	}
}

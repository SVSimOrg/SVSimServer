namespace Wizard;

public class AISetTribe : AIScriptArgumentExpressions
{
	private string _tribe;

	public AISetTribe(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList != null && _exprList.Count > 0 && _exprList[0].TokenList[0] is AIScriptTextToken aIScriptTextToken)
		{
			_tribe = aIScriptTextToken.Text;
		}
	}

	public bool CheckTribe(string tribe)
	{
		return tribe == _tribe;
	}
}

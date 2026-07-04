namespace Wizard;

public class AIFusion : AIFiltersArgument
{
	private AIPolishConvertedExpression _priority;

	private int PRIORITY_INDEX_OFFSET = 1;

	protected override int NON_FILTER_FIRST_OFFSET => PRIORITY_INDEX_OFFSET;

	public AIFusion(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_priority = _exprList[_exprList.Count - PRIORITY_INDEX_OFFSET];
	}

	public void SetFusionSituationParameter(AIFusionSituationInfo fusion)
	{
		fusion.SetParameter(base.Filters, _priority);
	}
}

namespace Wizard;

public class AIClashBuff : AIAttackBuff
{
	protected override int SELECT_TYPE_OFFSET => 0;

	protected override int NON_FILTER_FIRST_OFFSET => 3;

	public AIClashBuff(string text)
		: base(text)
	{
	}

	protected override void InitSelectType()
	{
		base.SelectType = AIScriptTokenArgType.ALL_SELECT;
	}
}

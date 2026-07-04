public class PpAddModifier : TurnAndIntValue, IPpModifier
{
	public PpAddModifier(int pp, int turn, bool isSelfTurn)
		: base(pp, turn, isSelfTurn)
	{
	}
}

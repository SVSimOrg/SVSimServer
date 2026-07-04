using System;

public class CostHalfRoundDownModifier : CostHalfModifier
{
	public CostHalfRoundDownModifier(bool isResidentModifier)
		: base(isResidentModifier)
	{
	}

	public override int CalcCost(int cost)
	{
		return (int)Math.Floor((float)cost / 2f);
	}

	public override ICardCostModifier Clone()
	{
		return new CostHalfRoundDownModifier(base.IsResidentModifier);
	}
}

using System;

public class CostHalfRoundUpModifier : CostHalfModifier
{
	public CostHalfRoundUpModifier(bool isResidentModifier)
		: base(isResidentModifier)
	{
	}

	public override int CalcCost(int cost)
	{
		return (int)Math.Ceiling((float)cost / 2f);
	}

	public override ICardCostModifier Clone()
	{
		return new CostHalfRoundUpModifier(base.IsResidentModifier);
	}
}

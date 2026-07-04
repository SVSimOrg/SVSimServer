using System.Collections.Generic;
using Wizard.Battle;

internal class BaseCardIDComp : EqualityComparer<IReadOnlyBattleCardInfo>
{
	public override bool Equals(IReadOnlyBattleCardInfo x, IReadOnlyBattleCardInfo y)
	{
		if (x == y)
		{
			return true;
		}
		if (x.BaseParameter.BaseCardId == y.BaseParameter.BaseCardId)
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode(IReadOnlyBattleCardInfo obj)
	{
		return obj.BaseParameter.BaseCardId;
	}
}

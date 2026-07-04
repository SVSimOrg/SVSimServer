using System.Collections.Generic;

namespace Wizard;

public class DiscardedVirtualCard : AIVirtualCard
{
	public DiscardedVirtualCard(BattleCardBase card, AIVirtualField field)
	{
		_field = field;
		base.IsAlly = true;
		InitializeFromBattleCardBase(card);
	}

	public override bool IsFollower(List<int> playPtn)
	{
		return IsUnit;
	}

	protected override void InitializeFromBattleCardBase(BattleCardBase origin)
	{
		InitializeFromBattleCardBaseBasic(origin);
		Cost = origin.Cost;
		IsPlayer = origin.IsPlayer;
		IsLeader = false;
		IsSelfTurn = origin.IsSelfTurn;
		base.DestroyedTurn = origin.DestroyedTurn;
	}
}

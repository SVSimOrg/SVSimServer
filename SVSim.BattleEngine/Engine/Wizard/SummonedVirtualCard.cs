using System.Collections.Generic;

namespace Wizard;

public class SummonedVirtualCard : AIVirtualCard
{
	public SummonedVirtualCard(BattleCardBase card, AIVirtualField field)
		: base(card, field)
	{
	}

	protected override void InitializeFromBattleCardBase(BattleCardBase origin)
	{
		InitializeFromBattleCardBaseBasic(origin);
		IsSpell = false;
		IsLeader = false;
		if (origin.Tribe != null && origin.Tribe.Count > 0)
		{
			for (int i = 0; i < origin.Tribe.Count; i++)
			{
				AppendTribe(origin.Tribe[i]);
			}
		}
	}

	public override bool IsFollower(List<int> playPtn)
	{
		return IsUnit;
	}
}

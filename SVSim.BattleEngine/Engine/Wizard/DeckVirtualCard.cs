using System.Collections.Generic;

namespace Wizard;

public class DeckVirtualCard : AIVirtualCard
{
	public DeckVirtualCard(BattleCardBase card, AIVirtualField field)
		: base(card, field)
	{
	}

	public DeckVirtualCard(DeckVirtualCard original, AIVirtualField field)
		: base(original, field)
	{
	}

	public override bool IsFollower(List<int> playPtn)
	{
		return IsUnit;
	}

	protected override void InitializeFromBattleCardBase(BattleCardBase origin)
	{
		base.CardIndex = origin.Index;
		InitializeFromBattleCardBaseBasic(origin);
		Cost = origin.Cost;
		IsPlayer = origin.IsPlayer;
		if (origin.Tribe != null && origin.Tribe.Count > 0)
		{
			for (int i = 0; i < origin.Tribe.Count; i++)
			{
				AppendTribe(origin.Tribe[i]);
			}
		}
	}
}

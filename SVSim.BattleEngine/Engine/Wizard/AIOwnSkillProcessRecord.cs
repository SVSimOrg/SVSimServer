using System.Collections.Generic;

namespace Wizard;

public class AIOwnSkillProcessRecord
{
	public List<AIVirtualCard> OwnDestroyedCards { get; private set; }

	public List<AIVirtualCard> OwnBanishedCards { get; private set; }

	public List<AIVirtualCard> OwnSummonedCards { get; private set; }

	public List<AIVirtualCard> OwnLatestSummonedCards { get; private set; }

	public List<AIVirtualCard> OwnLatestDrewCards { get; private set; }

	public List<AIVirtualCard> LatestTargets { get; private set; }

	public int DefaultDamage { get; private set; } = -1;

	public void AddOwnDestroyedCard(AIVirtualCard card)
	{
		OwnDestroyedCards = AIParamQuery.AddElementToList(card, OwnDestroyedCards);
	}

	public void AddOwnBanishedCard(AIVirtualCard card)
	{
		OwnBanishedCards = AIParamQuery.AddElementToList(card, OwnBanishedCards);
	}

	public void AddOwnSummonedCards(List<AIVirtualCard> list)
	{
		OwnSummonedCards = AIParamQuery.AddRangeToList(list, OwnSummonedCards);
		OwnLatestSummonedCards = list;
	}

	public void AddOwnDrewCards(List<AIVirtualCard> list)
	{
		OwnLatestDrewCards = list;
	}

	public void RegisterSingleLatestTarget(AIVirtualCard card)
	{
		if (LatestTargets != null)
		{
			LatestTargets.Clear();
		}
		else
		{
			LatestTargets = new List<AIVirtualCard>();
		}
		LatestTargets.Add(card);
	}

	public void RegisterLatestTargetList(List<AIVirtualCard> list)
	{
		LatestTargets = list;
	}

	public void RegisterDefaultDamage(int damage)
	{
		DefaultDamage = damage;
	}
}

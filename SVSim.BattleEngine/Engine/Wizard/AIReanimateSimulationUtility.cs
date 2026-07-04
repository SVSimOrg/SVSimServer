using System.Collections.Generic;
using System.Linq;
using Cute;

namespace Wizard;

public static class AIReanimateSimulationUtility
{

	public static bool IsReanimate(AIVirtualCard tagOwner, int cost)
	{
		AIVirtualField selfField = tagOwner.SelfField;
		return (tagOwner.IsAlly ? selfField.CardListSet.AllyDestroyedCards : selfField.CardListSet.EnemyDestroyedCards).Any((AIVirtualCard c) => c.IsUnit && c.Cost <= cost);
	}

	public static int GetReanimateTokenId(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<AIScriptTokenBase> filters, AIPolishConvertedExpression costExpression, AIScriptTokenArgType side, out bool isTokenAlly)
	{
		int num = (int)costExpression.EvalArg(tagOwner, playPtn, field, situation);
		if (num < 0)
		{
			isTokenAlly = true;
			return -1;
		}
		isTokenAlly = AISummonTokenUtility.GetIsTokenAlly(tagOwner, side);
		return EvalReanimateTokenID(tagOwner, num, filters, isTokenAlly, playPtn, situation);
	}

	private static int EvalReanimateTokenID(AIVirtualCard tagOwner, int reanimateCost, List<AIScriptTokenBase> filters, bool isTokenAlly, List<int> playPtn, AISituationInfo situation)
	{
		if (reanimateCost == -1)
		{
			return -1;
		}
		int result = -1;
		AIVirtualField selfField = tagOwner.SelfField;
		List<AIVirtualCard> list = (isTokenAlly ? selfField.CardListSet.AllyDestroyedCards : selfField.CardListSet.EnemyDestroyedCards);
		if (list.IsNotNullOrEmpty())
		{
			list = FilteringReanimateTargets(list, reanimateCost, tagOwner, filters, playPtn, situation);
			if (list == null || list.Count <= 0)
			{
				return result;
			}
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			for (int i = 0; i < list.Count; i++)
			{
				int baseId = list[i].BaseId;
				if (dictionary.ContainsKey(baseId))
				{
					dictionary[baseId]++;
				}
				else
				{
					dictionary.Add(baseId, 1);
				}
			}
			int maxDestroyedCount = dictionary.Max((KeyValuePair<int, int> pair) => pair.Value);
			if (dictionary.Count((KeyValuePair<int, int> pair) => pair.Value == maxDestroyedCount) == 1)
			{
				result = dictionary.First((KeyValuePair<int, int> pair) => pair.Value == maxDestroyedCount).Key;
			}
			else
			{
				float num = float.MaxValue;
				List<KeyValuePair<int, int>> maxDestroyedPair = dictionary.Where((KeyValuePair<int, int> pair) => pair.Value == maxDestroyedCount).ToList();
				int i2;
				for (i2 = 0; i2 < maxDestroyedPair.Count; i2++)
				{
					AIVirtualCard aIVirtualCard = list.First((AIVirtualCard c) => c.BaseId == maxDestroyedPair[i2].Key);
					float num2 = AIEvalReanimateUtility.EvalReanimateTargetValue(aIVirtualCard, selfField, playPtn, situation);
					num2 *= (isTokenAlly ? 1f : (-1f));
					if (num2 < num)
					{
						result = aIVirtualCard.BaseId;
						num = num2;
					}
				}
			}
		}
		return result;
	}

	public static List<AIVirtualCard> FilteringReanimateTargets(List<AIVirtualCard> reanimateTargets, int reanimateCost, AIVirtualCard tagOwner, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation)
	{
		if (reanimateTargets == null)
		{
			return null;
		}
		int maxReanimateCost = -1;
		List<AIVirtualCard> list = null;
		for (int i = 0; i < reanimateTargets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = reanimateTargets[i];
			int cost = aIVirtualCard.Cost;
			if (aIVirtualCard.IsUnit && cost <= reanimateCost && (filters == null || filters.Count < 0 || AIFilteringUtility.CheckMatchTargetFiltering(aIVirtualCard, reanimateTargets, filters, playPtn, tagOwner, situation)))
			{
				if (maxReanimateCost < cost)
				{
					maxReanimateCost = cost;
				}
				if (list == null)
				{
					list = new List<AIVirtualCard>();
				}
				list.Add(aIVirtualCard);
			}
		}
		if (list == null)
		{
			return null;
		}
		list.RemoveAll((AIVirtualCard c) => c.Cost != maxReanimateCost);
		return list;
	}
}

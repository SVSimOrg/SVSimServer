using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public static class AIHandPtnCalculator
{
	public class PriorityComparer : IComparer<int>
	{
		private EnemyAI _ai;

		private List<int> _playPtn;

		private List<float> _priorityList;

		public PriorityComparer(EnemyAI ai, List<int> playPtn)
		{
			_ai = ai;
			_playPtn = new List<int>(playPtn);
			CreatePriorityList();
		}

		public int Compare(int leftIndex, int rightIndex)
		{
			AIVirtualCard aIVirtualCard = _ai.CurrentVirtualField.AllyHandCards[leftIndex];
			AIVirtualCard aIVirtualCard2 = _ai.CurrentVirtualField.AllyHandCards[rightIndex];
			float num = _priorityList[leftIndex];
			float num2 = _priorityList[rightIndex];
			if (num > num2)
			{
				return -1;
			}
			if (num < num2)
			{
				return 1;
			}
			int cost = aIVirtualCard.Cost;
			int cost2 = aIVirtualCard2.Cost;
			if (cost < cost2)
			{
				return -1;
			}
			if (cost > cost2)
			{
				return 1;
			}
			if (leftIndex >= rightIndex)
			{
				return 1;
			}
			return -1;
		}

		private void CreatePriorityList()
		{
			List<AIVirtualCard> allyHandCards = _ai.CurrentVirtualField.AllyHandCards;
			_priorityList = new List<float>();
			for (int i = 0; i < allyHandCards.Count; i++)
			{
				AIVirtualCard card = allyHandCards[i];
				_priorityList.Add(card.GetPriority(_playPtn));
			}
		}
	}

	public static List<Tuple<int, List<int>>> CreateSortedPlayPtnList(EnemyAI ai)
	{
		List<Tuple<int, List<int>>> list = new List<Tuple<int, List<int>>>();
		int count = ai.CurrentVirtualField.AllyHandCards.Count;
		int num = (int)Mathf.Pow(2f, count);
		List<int> list2 = new List<int>();
		for (int i = 0; i < num; i++)
		{
			ConvertHandPtnIndexToList(i, count, list2);
			PrioritySortHand(ai, list2);
			Tuple<int, List<int>> item = new Tuple<int, List<int>>
			{
				first = i,
				second = new List<int>(list2)
			};
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].second.Count > list2.Count)
				{
					flag = true;
					list.Insert(j, item);
					break;
				}
			}
			if (!flag)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static void ConvertHandPtnIndexToList(int handPtnIndex, int handCount, List<int> dstList)
	{
		int num = handPtnIndex;
		dstList.Clear();
		for (int i = 0; i < handCount; i++)
		{
			int num2 = (int)Mathf.Pow(2f, handCount - i - 1);
			if (num / num2 > 0)
			{
				num -= num2;
				dstList.Add(i);
			}
		}
	}

	public static void PrioritySortHand(EnemyAI ai, List<int> handList)
	{
		IComparer<int> comparer = new PriorityComparer(ai, handList);
		handList.Sort(comparer);
	}

	public static ulong CalculatePlayPtnHash(AIVirtualField field, List<int> playPtn)
	{
		if (playPtn == null || playPtn.Count <= 0)
		{
			return 0uL;
		}
		ulong num = 0uL;
		for (int i = 0; i < playPtn.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyHandCards[playPtn[i]];
			num += (ulong)((long)aIVirtualCard.GetHash() * (long)(i + 1));
		}
		return num;
	}
}

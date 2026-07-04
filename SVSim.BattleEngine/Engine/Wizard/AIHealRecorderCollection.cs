using System.Collections.Generic;

namespace Wizard;

public class AIHealRecorderCollection
{
	public class AIHealRecorder
	{
		public int TurnCount { get; private set; }

		public AIVirtualCard Card { get; private set; }

		public AIHealRecorder(int turnCount, AIVirtualCard healedCard)
		{
			TurnCount = turnCount;
			Card = healedCard;
		}
	}

	public List<AIHealRecorder> AllyHealRecorderList { get; private set; }

	public List<AIHealRecorder> EnemyHealRecorderList { get; private set; }

	public AIHealRecorderCollection()
	{
		AllyHealRecorderList = new List<AIHealRecorder>();
		EnemyHealRecorderList = new List<AIHealRecorder>();
	}

	private AIHealRecorderCollection(AIHealRecorderCollection original)
	{
		AllyHealRecorderList = new List<AIHealRecorder>(original.AllyHealRecorderList);
		EnemyHealRecorderList = new List<AIHealRecorder>(original.EnemyHealRecorderList);
	}

	public AIHealRecorderCollection Clone()
	{
		return new AIHealRecorderCollection(this);
	}

	public int GetTurnHealCount(int turn, List<AIVirtualCard> checkTargets, bool isAlly)
	{
		List<AIHealRecorder> list = (isAlly ? AllyHealRecorderList : EnemyHealRecorderList);
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			AIHealRecorder aIHealRecorder = list[i];
			if (aIHealRecorder.TurnCount != turn)
			{
				continue;
			}
			for (int j = 0; j < checkTargets.Count; j++)
			{
				if (aIHealRecorder.Card.IsSameCard(checkTargets[j]))
				{
					num++;
					break;
				}
			}
		}
		return num;
	}

	public void AppendHealCount(int turn, AIVirtualCard healedCard, bool isAlly)
	{
		(isAlly ? AllyHealRecorderList : EnemyHealRecorderList).Add(new AIHealRecorder(turn, healedCard));
	}
}

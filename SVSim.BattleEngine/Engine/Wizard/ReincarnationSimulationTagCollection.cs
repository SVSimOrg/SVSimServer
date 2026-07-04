using System.Collections.Generic;

namespace Wizard;

public class ReincarnationSimulationTagCollection : TagCollection
{
	public ReincarnationSimulationTagCollection()
		: base(TagCollectionType.Reincarnation)
	{
	}

	private ReincarnationSimulationTagCollection(ReincarnationSimulationTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new ReincarnationSimulationTagCollection(this);
	}

	public float CalcReincarnationValueAfterSimulation(EnemyAI ai, List<int> playPtn, AISituationInfo situation, List<AIVirtualCard> selfRemainings, List<AIVirtualCard> opponentRemainings)
	{
		float num = 0f;
		_ = playPtn.Count;
		if (base.TagList.Count > 0)
		{
			for (int i = 0; i < selfRemainings.Count; i++)
			{
				AIVirtualCard target = selfRemainings[i];
				float num2 = CalcReincarnationValueToVirtualCard(ai, target, playPtn, situation);
				if (num2 > num)
				{
					num = num2;
				}
			}
			int count = opponentRemainings.Count;
			for (int j = 0; j < count; j++)
			{
				AIVirtualCard target2 = opponentRemainings[j];
				float num3 = 0f - CalcReincarnationValueToVirtualCard(ai, target2, playPtn, situation);
				if (num3 > num)
				{
					num = num3;
				}
			}
		}
		return num;
	}

	public float CalcReincarnationValueToVirtualCard(EnemyAI ai, AIVirtualCard target, List<int> playPtn, AISituationInfo situation)
	{
		float num = 0f - target.EvaluateValueOnField(playPtn, situation, useStyle: true, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle: true) + target.EvaluateBreakValue(playPtn, useIgnoreBreak: true) + target.EvaluateLeaveValue(playPtn, useIgnoreInBattle: true);
		float num2 = AIReincarnationUtility.EvaluateFollowerPrimaryValue(target, playPtn, useStyle: true) * target.EvaluateAllBattleBonusRate(playPtn, useOthersTag: true, useIgnoreInBattle: true, situation) + target.GetFieldBonus(playPtn) + (target.EvaluateBreakValue(playPtn, useIgnoreBreak: true) + target.EvaluateLeaveValue(playPtn, useIgnoreInBattle: true)) * EnemyAI.BREAKBONUS_RATE_IN_HAND;
		return num + num2;
	}
}

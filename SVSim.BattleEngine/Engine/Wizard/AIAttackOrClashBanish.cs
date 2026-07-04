using System.Collections.Generic;

namespace Wizard;

public class AIAttackOrClashBanish : AIWhenAttackOrWhenFightTagArgument
{
	public override bool IsActivateWhenEvalInstantAttack => true;

	public AIAttackOrClashBanish(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIBanishSimulationUtility.BanishAll(targetsFromField, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AIBanishSimulationUtility.BanishRandom(targetsFromField, tagOwner, field, playPtn, situation);
				break;
			}
		}
	}

	public override bool CanKillTarget(AIVirtualCard tagOwner, AIVirtualCard target, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, AIBarrierPseudoSimulationInfo simBarrier, ref int totalDamage)
	{
		if (target.IsIndependent || target.IsUnbanishable)
		{
			return false;
		}
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Contains(target))
		{
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				return true;
			}
			if (base.SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				AIVirtualCard card = AISimulationRemovalUtility.SelectRemovalTarget(targetsFromField, tagOwner, field, field.BestPlayPtn, situation, AISelectTargetPattern.Worst, AIRemovalType.Banish);
				return target.IsSameCard(card);
			}
		}
		return false;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}

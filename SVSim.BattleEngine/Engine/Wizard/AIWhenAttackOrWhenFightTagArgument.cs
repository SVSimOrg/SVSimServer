using System.Collections.Generic;

namespace Wizard;

public class AIWhenAttackOrWhenFightTagArgument : AIFiltersAndSelectTypeArgument
{
	public virtual bool IsActivateWhenEvalInstantAttack => false;

	public AIWhenAttackOrWhenFightTagArgument(string text)
		: base(text)
	{
	}

	public virtual bool CanKillTarget(AIVirtualCard tagOwner, AIVirtualCard target, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, AIBarrierPseudoSimulationInfo simBarrier, ref int totalDamage)
	{
		return false;
	}

	public virtual bool CanKillAnyTarget(AIVirtualCard tagOwner, List<AIVirtualCard> targetList, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, List<AIBarrierPseudoSimulationInfo> simBarrierList, int[] realDamageList)
	{
		return false;
	}

	public virtual int PseudoSimulateWhenAttackDamageToCertainCard(AIVirtualCard tagOwner, AIVirtualCard damageTarget, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, AIBarrierPseudoSimulationInfo simBarrier)
	{
		return 0;
	}

	public virtual void PseudoSimulateForEvalInstantAttack(AIVirtualCard tagOwner, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, EvalInstantAttackInformation information)
	{
		AIBarrierPseudoSimulationInfo attackerBarrierInfo = information.AttackerBarrierInfo;
		int totalDamage = information.AttackerTotalDamage;
		if (CanKillTarget(tagOwner, situation.Actor, field, situation, playPtn, attackerBarrierInfo, ref totalDamage))
		{
			information.IsAttackerDestroyWhenAttack = true;
			information.AttackerTotalDamage = totalDamage;
		}
	}
}

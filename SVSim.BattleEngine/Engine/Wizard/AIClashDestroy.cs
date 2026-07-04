using System.Collections.Generic;

namespace Wizard;

public class AIClashDestroy : AIWhenAttackOrWhenFightTagArgument
{
	public override bool IsActivateWhenEvalInstantAttack => true;

	protected override int SELECT_TYPE_OFFSET => 0;

	public AIClashDestroy(string text)
		: base(text)
	{
	}

	protected override void InitSelectType()
	{
		base.SelectType = AIScriptTokenArgType.ALL_SELECT;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < targetsFromField.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targetsFromField[i];
			if (!aIVirtualCard.IsIndependent && !aIVirtualCard.IsDead)
			{
				aIVirtualCard.RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: true);
			}
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	public override bool CanKillTarget(AIVirtualCard tagOwner, AIVirtualCard target, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, AIBarrierPseudoSimulationInfo simBarrier, ref int totalDamage)
	{
		if (target.IsIndependent || target.IsIndestructible)
		{
			return false;
		}
		return GetTargetsFromField(tagOwner, field, playPtn, situation)?.Contains(target) ?? false;
	}
}

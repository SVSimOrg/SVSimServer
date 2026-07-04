using System.Collections.Generic;

namespace Wizard;

public abstract class AIAttackOrClashBarrierBase : AIWhenAttackOrWhenFightTagArgument
{
	protected AIScriptTokenArgType _stopTiming;

	protected AIScriptTokenArgType _damageType;

	protected bool _isDamageTypeDefinedByMaster;

	protected abstract int _defaultDamageTypeOffset { get; }

	protected abstract int _stopTimingOffset { get; }

	protected override int SELECT_TYPE_OFFSET => 1 + (_isDamageTypeDefinedByMaster ? _defaultDamageTypeOffset : _stopTimingOffset);

	public override bool IsActivateWhenEvalInstantAttack => true;

	public AIAttackOrClashBarrierBase(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		_stopTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - _stopTimingOffset]);
		_damageType = AIPlayTagInitializingUtility.GetDamageTypeFromExprList(_exprList[_exprList.Count - _defaultDamageTypeOffset], out _isDamageTypeDefinedByMaster);
		InitSelectType();
		InitializeFilter();
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0 && base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			GiveBarrierToAllTargets(targetsFromField, tagOwner, field, playPtn, situation);
		}
	}

	public override bool CanKillTarget(AIVirtualCard tagOwner, AIVirtualCard target, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, AIBarrierPseudoSimulationInfo simBarrier, ref int totalDamage)
	{
		PseudoSimulateWhenAttackDamageToCertainCard(tagOwner, target, field, situation, playPtn, simBarrier);
		return false;
	}

	public override int PseudoSimulateWhenAttackDamageToCertainCard(AIVirtualCard tagOwner, AIVirtualCard damageTarget, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, AIBarrierPseudoSimulationInfo simBarrier)
	{
		if (situation == null || situation.ActionType != AIOperationType.ATTACK)
		{
			return 0;
		}
		if (AIFilteringUtility.CheckMatchTargetFiltering(simBarrier.Owner, null, base.Filters, playPtn, tagOwner, situation))
		{
			PseudoGiveBarrierToCertainTarget(simBarrier, tagOwner, field, playPtn, situation);
		}
		return 0;
	}

	protected abstract void GiveBarrierToAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation);

	protected abstract void PseudoGiveBarrierToCertainTarget(AIBarrierPseudoSimulationInfo simBarrier, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation);
}

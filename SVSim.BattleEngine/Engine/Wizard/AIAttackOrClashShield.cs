using System.Collections.Generic;

namespace Wizard;

public class AIAttackOrClashShield : AIAttackOrClashBarrierBase
{
	private List<AIScriptTokenArgType> _stopTimingList;

	protected override int _defaultDamageTypeOffset => 2;

	protected override int _stopTimingOffset => 1;

	public AIAttackOrClashShield(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		_stopTimingList = AIPlayTagInitializingUtility.InitializeStopTimingList(_exprList[_exprList.Count - _stopTimingOffset]);
		_damageType = AIPlayTagInitializingUtility.GetDamageTypeFromExprList(_exprList[_exprList.Count - _defaultDamageTypeOffset], out _isDamageTypeDefinedByMaster);
		InitSelectType();
		InitializeFilter();
	}

	protected override void GiveBarrierToAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		AIBarrierSimulationUtility.AddMultipleStopTimingShieldToAll(targets, tagOwner, field, _damageType, _stopTimingList);
	}

	protected override void PseudoGiveBarrierToCertainTarget(AIBarrierPseudoSimulationInfo simBarrier, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		AIDamageType damageTypeFromArgType = AIBarrierSimulationUtility.GetDamageTypeFromArgType(_damageType);
		List<AIBarrierStopTiming> barrierStopTimingListFromArgType = AIBarrierSimulationUtility.GetBarrierStopTimingListFromArgType(_stopTimingList, tagOwner);
		AIBarrierSimulationUtility.PseudoAddMultipleStopTimingShieldToSingle(simBarrier, damageTypeFromArgType, barrierStopTimingListFromArgType);
	}

	public override void PseudoSimulateForEvalInstantAttack(AIVirtualCard tagOwner, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, EvalInstantAttackInformation information)
	{
		AIDamageType damageTypeFromArgType = AIBarrierSimulationUtility.GetDamageTypeFromArgType(_damageType);
		List<AIBarrierStopTiming> barrierStopTimingListFromArgType = AIBarrierSimulationUtility.GetBarrierStopTimingListFromArgType(_stopTimingList, tagOwner);
		if (AIFilteringUtility.CheckMatchTargetFiltering(information.AttackerBarrierInfo.Owner, null, base.Filters, playPtn, tagOwner, situation))
		{
			AIBarrierSimulationUtility.PseudoAddMultipleStopTimingShieldToSingle(information.AttackerBarrierInfo, damageTypeFromArgType, barrierStopTimingListFromArgType);
		}
		if (AIFilteringUtility.CheckMatchTargetFiltering(information.TargetBarrierInfo.Owner, null, base.Filters, playPtn, tagOwner, situation))
		{
			AIBarrierSimulationUtility.PseudoAddMultipleStopTimingShieldToSingle(information.TargetBarrierInfo, damageTypeFromArgType, barrierStopTimingListFromArgType);
		}
	}
}

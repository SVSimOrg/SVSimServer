using System.Collections.Generic;

namespace Wizard;

public class AIAttackOrClashDamageClip : AIAttackOrClashBarrierBase
{
	private AIPolishConvertedExpression _clipAmount;

	protected override int _defaultDamageTypeOffset => 3;

	protected override int _stopTimingOffset => 2;

	public AIAttackOrClashDamageClip(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_clipAmount = _exprList[_exprList.Count - 1];
	}

	protected override void GiveBarrierToAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		int clipAmount = (int)_clipAmount.EvalArg(tagOwner, playPtn, field, situation);
		AIBarrierSimulationUtility.AddDamageClipToAll(targets, tagOwner, field, _damageType, _stopTiming, clipAmount);
	}

	protected override void PseudoGiveBarrierToCertainTarget(AIBarrierPseudoSimulationInfo simBarrier, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		int clipAmount = (int)_clipAmount.EvalArg(tagOwner, playPtn, field, situation);
		AIDamageType damageTypeFromArgType = AIBarrierSimulationUtility.GetDamageTypeFromArgType(_damageType);
		AIBarrierStopTiming barrierStopTimingFromArgType = AIBarrierSimulationUtility.GetBarrierStopTimingFromArgType(_stopTiming, tagOwner);
		AIBarrierSimulationUtility.PseudoAddDamageClipToSingle(simBarrier, damageTypeFromArgType, barrierStopTimingFromArgType, clipAmount);
	}

	public override void PseudoSimulateForEvalInstantAttack(AIVirtualCard tagOwner, AIVirtualField field, AIVirtualAttackInfo situation, List<int> playPtn, EvalInstantAttackInformation information)
	{
		int clipAmount = (int)_clipAmount.EvalArg(tagOwner, playPtn, field, situation);
		AIDamageType damageTypeFromArgType = AIBarrierSimulationUtility.GetDamageTypeFromArgType(_damageType);
		AIBarrierStopTiming barrierStopTimingFromArgType = AIBarrierSimulationUtility.GetBarrierStopTimingFromArgType(_stopTiming, tagOwner);
		if (AIFilteringUtility.CheckMatchTargetFiltering(information.AttackerBarrierInfo.Owner, null, base.Filters, playPtn, tagOwner, situation))
		{
			AIBarrierSimulationUtility.PseudoAddDamageClipToSingle(information.AttackerBarrierInfo, damageTypeFromArgType, barrierStopTimingFromArgType, clipAmount);
		}
		if (AIFilteringUtility.CheckMatchTargetFiltering(information.TargetBarrierInfo.Owner, null, base.Filters, playPtn, tagOwner, situation))
		{
			AIBarrierSimulationUtility.PseudoAddDamageClipToSingle(information.TargetBarrierInfo, damageTypeFromArgType, barrierStopTimingFromArgType, clipAmount);
		}
	}
}

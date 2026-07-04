using System.Collections.Generic;

namespace Wizard;

public class AIChangeInplayImmediateDamageCut : AIChangeInplayImmediateBarrierBase
{
	private AIPolishConvertedExpression _cutAmount;

	protected override int _stopTimingOffset => 2;

	protected override int _defaultDamageTypeOffset => 3;

	public AIChangeInplayImmediateDamageCut(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_cutAmount = _exprList[_exprList.Count - 1];
	}

	protected override void GiveBarrierToAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		int cutAmount = (int)_cutAmount.EvalArg(tagOwner, playPtn, field, situation);
		AIBarrierSimulationUtility.AddDamageCutToAll(targets, tagOwner, field, _damageType, _stopTiming, cutAmount);
	}

	protected override void DepriveBarrierFromAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		AIDamageType damageTypeFromArgType = AIBarrierSimulationUtility.GetDamageTypeFromArgType(_damageType);
		AIBarrierStopTiming barrierStopTimingFromArgType = AIBarrierSimulationUtility.GetBarrierStopTimingFromArgType(_stopTiming, tagOwner);
		int cutAmount = (int)_cutAmount.EvalArg(tagOwner, playPtn, field, situation);
		ulong depriveShieldHash = AIBarrierSimulationUtility.CalculateDamageCutInfoHash(damageTypeFromArgType, AIBarrierType.Shield, barrierStopTimingFromArgType, cutAmount);
		for (int i = 0; i < targets.Count; i++)
		{
			targets[i].BarrierInfoCollection.DepriveCertainBarrier(depriveShieldHash, barrierStopTimingFromArgType);
		}
	}
}

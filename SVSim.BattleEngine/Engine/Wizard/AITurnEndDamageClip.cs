using System.Collections.Generic;

namespace Wizard;

public class AITurnEndDamageClip : AITurnEndBarrierBase
{
	private AIPolishConvertedExpression _clipAmount;

	protected override int _defaultDamageTypeOffset => 4;

	protected override int _stopTimingOffset => 3;

	public AITurnEndDamageClip(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_clipAmount = _exprList[_exprList.Count - 2];
	}

	protected override void GiveBarrierToAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		int clipAmount = (int)_clipAmount.EvalArg(tagOwner, playPtn, field, situation);
		AIBarrierSimulationUtility.AddDamageClipToAll(targets, tagOwner, field, _damageType, _stopTiming, clipAmount);
	}
}

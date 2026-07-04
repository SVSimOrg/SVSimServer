using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayDamageClip : AIWhenPlayBarrierBase
{
	private AIPolishConvertedExpression _clipAmount;

	protected override int _defaultDamageTypeOffset => 3;

	protected override int _stopTimingOffset => 2;

	public AIWhenPlayDamageClip(string text)
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
}

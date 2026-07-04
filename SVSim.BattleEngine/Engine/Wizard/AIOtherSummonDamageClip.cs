using System.Collections.Generic;

namespace Wizard;

public class AIOtherSummonDamageClip : AIOtherSummonBarrierBase
{
	private AIPolishConvertedExpression _clipAmount;

	protected override int _defaultDamageTypeOffset => 3;

	protected override int _stopTimingOffset => 2;

	public AIOtherSummonDamageClip(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_clipAmount = _exprList[_exprList.Count - 1];
	}

	protected override void GiveBarrierToAllTargets(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		int clipAmount = (int)_clipAmount.EvalArg(tagOwner, playPtn, field, situation);
		AIBarrierSimulationUtility.AddDamageClipToAll(targets, tagOwner, field, _damageType, _stopTiming, clipAmount);
	}
}

using System.Collections.Generic;

namespace Wizard;

public class AIChangeInplayImmediateDamageClip : AIChangeInplayImmediateBarrierBase
{
	private AIPolishConvertedExpression _clipAmount;

	private AIPolishConvertedExpression _clipRange;

	private bool _isClipRangeDefinedByMaster;

	protected override int _stopTimingOffset
	{
		get
		{
			if (!_isClipRangeDefinedByMaster)
			{
				return 2;
			}
			return 3;
		}
	}

	protected override int _defaultDamageTypeOffset
	{
		get
		{
			if (!_isClipRangeDefinedByMaster)
			{
				return 3;
			}
			return 4;
		}
	}

	public AIChangeInplayImmediateDamageClip(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		InitClipAmountAndClipRange();
		_stopTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - _stopTimingOffset]);
		_damageType = AIPlayTagInitializingUtility.GetDamageTypeFromExprList(_exprList[_exprList.Count - _defaultDamageTypeOffset], out _isDamageTypeDefinedByMaster);
		InitSelectType();
		InitializeFilter();
	}

	private void InitClipAmountAndClipRange()
	{
		AIPolishConvertedExpression aIPolishConvertedExpression = _exprList[_exprList.Count - 1];
		AIPolishConvertedExpression aIPolishConvertedExpression2 = _exprList[_exprList.Count - 2];
		if (aIPolishConvertedExpression2.IsMathematicExpress())
		{
			_isClipRangeDefinedByMaster = true;
			_clipAmount = aIPolishConvertedExpression2;
			_clipRange = aIPolishConvertedExpression;
		}
		else
		{
			_clipAmount = aIPolishConvertedExpression;
			_clipRange = null;
		}
	}

	protected override void GiveBarrierToAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		int clipAmount = (int)_clipAmount.EvalArg(tagOwner, playPtn, field, situation);
		int clipRange = GetClipRange(tagOwner, field, playPtn, situation);
		AIBarrierSimulationUtility.AddDamageClipToAll(targets, tagOwner, field, _damageType, _stopTiming, clipAmount, clipRange);
	}

	protected override void DepriveBarrierFromAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		AIDamageType damageTypeFromArgType = AIBarrierSimulationUtility.GetDamageTypeFromArgType(_damageType);
		AIBarrierStopTiming barrierStopTimingFromArgType = AIBarrierSimulationUtility.GetBarrierStopTimingFromArgType(_stopTiming, tagOwner);
		int clipAmount = (int)_clipAmount.EvalArg(tagOwner, playPtn, field, situation);
		int clipRange = GetClipRange(tagOwner, field, playPtn, situation);
		ulong depriveShieldHash = AIBarrierSimulationUtility.CalculateDamageClipInfoHash(damageTypeFromArgType, AIBarrierType.DamageClipping, barrierStopTimingFromArgType, clipAmount, clipRange);
		for (int i = 0; i < targets.Count; i++)
		{
			targets[i].BarrierInfoCollection.DepriveCertainBarrier(depriveShieldHash, barrierStopTimingFromArgType);
		}
	}

	private int GetClipRange(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_clipRange == null)
		{
			return 9999;
		}
		return (int)_clipRange.EvalArg(tagOwner, playPtn, field, situation);
	}
}

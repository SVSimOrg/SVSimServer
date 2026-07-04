using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class AIDelayTurnEndTime : AIScriptArgumentExpressions
{
	private AIPolishConvertedExpression _minDelayTime;

	private AIPolishConvertedExpression _maxDelayTime;

	private int MIN_DELAY_TIME_ARG_INDEX;

	private int MAX_DELAY_TIME_ARG_INDEX = 1;

	public AIDelayTurnEndTime(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList.Count > MAX_DELAY_TIME_ARG_INDEX)
		{
			_minDelayTime = _exprList[MIN_DELAY_TIME_ARG_INDEX];
			_maxDelayTime = _exprList[MAX_DELAY_TIME_ARG_INDEX];
		}
	}

	public float GetDelayTime(AIVirtualCard ownerCard, List<int> playPtn)
	{
		if (_minDelayTime == null || _maxDelayTime == null)
		{
			return 0f;
		}
		AIVirtualField selfField = ownerCard.SelfField;
		float minInclusive = _minDelayTime.EvalArg(ownerCard, playPtn, selfField);
		float maxInclusive = _maxDelayTime.EvalArg(ownerCard, playPtn, selfField);
		return Random.Range(minInclusive, maxInclusive);
	}
}

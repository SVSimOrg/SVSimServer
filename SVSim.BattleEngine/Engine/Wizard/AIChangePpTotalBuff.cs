using System.Collections.Generic;

namespace Wizard;

public class AIChangePpTotalBuff : AIWhenChangeInplayTagArgument
{

	public AIPolishConvertedExpression Attack { get; private set; }

	public AIPolishConvertedExpression Life { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 3;

	public AIChangePpTotalBuff(string text, bool isImmediate)
		: base(text, isImmediate)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		Attack = _exprList[_exprList.Count - 2];
		Life = _exprList[_exprList.Count - 1];
	}

	private void BuffTargets(List<AIVirtualCard> targets, AIBuffExecutingInfo_old buffInfo, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (targets != null && targets.Count > 0)
		{
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIBuffSimulationUtility.BuffAll_old(targets, field, buffInfo, isTemp: false, playPtn, situation);
			}
			else
			{
				AIConsoleUtility.LogError($"AIChangePpTotalBuff.BuffTargets(): SelectType is Ileagl [type:{base.SelectType}]");
			}
		}
	}

	protected override void ChangeInplayTagStartProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, Attack, Life);
		BuffTargets(targets, buffExecutingInfo_old, tagOwner, field, playPtn, situation);
	}

	protected override void ChangeInplayTagStopProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, Attack, Life);
		if (!buffExecutingInfo_old.IsMultiplyAttack && !buffExecutingInfo_old.IsMultiplyLife)
		{
			buffExecutingInfo_old.AttackValue *= -1;
			buffExecutingInfo_old.LifeValue *= -1;
			BuffTargets(targets, buffExecutingInfo_old, tagOwner, field, playPtn, situation);
		}
	}
}

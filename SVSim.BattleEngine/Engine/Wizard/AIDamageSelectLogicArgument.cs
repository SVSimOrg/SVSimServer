using System.Collections.Generic;

namespace Wizard;

public class AIDamageSelectLogicArgument : AISelectLogicArgumentBase
{

	public override AIScriptTokenArgType LogicType => AIScriptTokenArgType.DAMAGE_LOGIC;

	public AIDamageSelectLogicArgument(List<string> args)
		: base(args)
	{
	}

	public override AIVirtualCard SelectSingleTarget(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		int damageValue = GetDamageValue(tagOwner, field, playPtn, situation);
		return AIDamageSimulationUtility.SelectDamageTarget(candidates, field, playPtn, situation, damageValue, tagOwner.IsSpell, worstOrBest);
	}

	public override List<AIVirtualCard> SelectMultipleSelectedTargets(List<AIVirtualCard> candidates, int selectCount, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		LogNotImplementMultipleSelect();
		return null;
	}

	private int GetDamageValue(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_argumentList == null || _argumentList.Count <= 0)
		{
			AIConsoleUtility.LogError("AIDamageSelectLogicArgument error!! _argumentList is null");
			return 0;
		}
		return (int)_argumentList[0].EvalArg(tagOwner, playPtn, field, situation);
	}
}

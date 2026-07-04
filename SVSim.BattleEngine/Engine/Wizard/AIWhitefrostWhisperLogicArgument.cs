using System.Collections.Generic;

namespace Wizard;

public class AIWhitefrostWhisperLogicArgument : AISelectLogicArgumentBase
{

	public override AIScriptTokenArgType LogicType => AIScriptTokenArgType.WHITEFROST_WHISPER_LOGIC;

	public AIWhitefrostWhisperLogicArgument(List<string> args)
		: base(args)
	{
	}

	public override AIVirtualCard SelectSingleTarget(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		float num = float.MinValue;
		AIVirtualCard result = null;
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			if (aIVirtualCard.IsDead)
			{
				AIConsoleUtility.LogError("AIWhitefrostWhisperLogicArgument.SelectSingleTarget() error!! candidate is already dead!!!!!");
				continue;
			}
			float num2 = ((aIVirtualCard.Life >= aIVirtualCard.MaxLife) ? AIDamageSimulationUtility.GetDamageValueToCertainTarget(aIVirtualCard, 1, situation, playPtn, tagOwner.IsSpell) : ((!aIVirtualCard.IsIndependent && !aIVirtualCard.IsIndestructible) ? AIDestroySimulationUtility.CalcEvalDestroy(aIVirtualCard, playPtn, situation, useIgnoreBreak: false) : 0f));
			if (num2 > num)
			{
				num = num2;
				result = aIVirtualCard;
			}
		}
		return result;
	}

	public override List<AIVirtualCard> SelectMultipleSelectedTargets(List<AIVirtualCard> candidates, int selectCount, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		AIConsoleUtility.LogError("AIWhitefrostWhisperLogicArgument.SelectMultipleSelectedTargets() error!! Not implemented!!!!!");
		return null;
	}
}

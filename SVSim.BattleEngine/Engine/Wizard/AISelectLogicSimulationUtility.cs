using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public static class AISelectLogicSimulationUtility
{
	public static AISelectLogicArgumentBase CreateSelectLogicArgument(string args)
	{
		List<string> list = AIPlayTagInitializingUtility.TrimAttachTagArgument(args).Replace(" ", "").Split(';')
			.ToList();
		AIScriptTokenArgType aIScriptTokenArgType = AIScriptTokenArgType.NONE;
		if (AIScriptParser.ConvertWordToToken(list[0]) is AIScriptArgumentToken aIScriptArgumentToken)
		{
			aIScriptTokenArgType = aIScriptArgumentToken.ArgumentType;
		}
		if (aIScriptTokenArgType <= AIScriptTokenArgType.SELECT_LOGIC_TYPE_BEGIN || aIScriptTokenArgType >= AIScriptTokenArgType.SELECT_LOGIC_TYPE_END)
		{
			AIConsoleUtility.LogError($"CreateSelectLogicArgument error!! logicType == {aIScriptTokenArgType}");
			return null;
		}
		list.RemoveAt(0);
		return aIScriptTokenArgType switch
		{
			AIScriptTokenArgType.DEFAULT_LOGIC => new AIDefaultSelectLogicArgument(list), 
			AIScriptTokenArgType.DESTROY_LOGIC => new AIDestroySelectLogicArgument(list), 
			AIScriptTokenArgType.DAMAGE_LOGIC => new AIDamageSelectLogicArgument(list), 
			AIScriptTokenArgType.BOUNCE_LOGIC => new AIBounceSelectLogicArgument(list), 
			AIScriptTokenArgType.BANISH_LOGIC => new AIBanishSelectLogicArgument(list), 
			AIScriptTokenArgType.METAMORPHOSE_LOGIC => new AIMetamorphoseSelectLogicArgument(list), 
			AIScriptTokenArgType.MAX_ATTACK_LOGIC => new AIMaxAttackSelectLogicArgument(list), 
			AIScriptTokenArgType.TYRANT_ORDER_LOGIC => new AITyrantOrderSelectLogicArgument(list), 
			AIScriptTokenArgType.REVERSE_DISCARD_LOGIC => new AIReverseDiscardSelectLogicArgument(list), 
			AIScriptTokenArgType.WHITEFROST_WHISPER_LOGIC => new AIWhitefrostWhisperLogicArgument(list), 
			_ => null, 
		};
	}
}

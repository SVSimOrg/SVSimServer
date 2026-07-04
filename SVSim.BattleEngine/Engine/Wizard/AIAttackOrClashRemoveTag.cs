using System.Collections.Generic;

namespace Wizard;

public class AIAttackOrClashRemoveTag : AIWhenAttackOrWhenFightTagArgument, IAIRemoveTagArgument
{

	public AIPlayTag RemoveTag { get; private set; }

	public AIAttackOrClashRemoveTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		if (list == null || list.Count < 3)
		{
			AIConsoleUtility.LogError("AIClashRemoveTag error!! splitedText.Length is not enough");
		}
		else
		{
			RemoveTag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[0], list[1], list[2]);
		}
	}

	public void Execute(AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation, AIPlayTag ownerTag, List<int> playPtn = null)
	{
		AIRemoveTagUtility.RemoveOneTag(tagOwner, field, RemoveTag, situation);
		AIRemoveTagUtility.RemoveOneTag(tagOwner, field, ownerTag, situation);
	}
}

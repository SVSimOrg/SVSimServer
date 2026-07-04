using System.Collections.Generic;

namespace Wizard;

public class AIOtherAttackRemoveTag : AIWhenAttackSelfAndOtherTagArgument, IAIRemoveTagArgument
{

	public AIPlayTag RemoveTag { get; private set; }

	public AIOtherAttackRemoveTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		if (list == null || list.Count < 4)
		{
			AIConsoleUtility.LogError("AIOtherAttackRemoveTag error!! splitedText.Length is not enough");
			return;
		}
		base.InitExpressions(list[0]);
		RemoveTag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[1], list[2], list[3]);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}

	public void Execute(AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation, AIPlayTag executingOwnerTag, List<int> playPtn = null)
	{
		AIVirtualCard aIVirtualCard = ((situation != null && situation.ActionType == AIOperationType.ATTACK) ? situation.Actor : null);
		if (aIVirtualCard != null && CheckTriggerLegal(aIVirtualCard, tagOwner, playPtn, situation))
		{
			AIRemoveTagUtility.RemoveOneTag(tagOwner, field, RemoveTag, situation);
			AIRemoveTagUtility.RemoveOneTag(tagOwner, field, executingOwnerTag, situation);
		}
	}
}

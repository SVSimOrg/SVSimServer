using System.Collections.Generic;

namespace Wizard;

public class AITurnEndRemoveTag : AIScriptArgumentExpressions, IAITurnEndArgument, IAIRemoveTagArgument
{

	public AIPlayTag RemoveTag { get; private set; }

	public bool IsAllyTurn { get; private set; }

	public AITurnEndRemoveTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		if (list == null || list.Count < 4)
		{
			AIConsoleUtility.LogError("AITurnEndRemoveTag error!! splitedText.Length is not enough");
			return;
		}
		RemoveTag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[1], list[2], list[3]);
		base.InitExpressions(list[0]);
		IsAllyTurn = TurnEndTagCollection.IsAllyTurn(_exprList, GetType(), _exprList.Count - 1);
	}

	public float CalculateThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allyInplayStatusList)
	{
		return 0f;
	}

	public void Execute(AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation, AIPlayTag executingOwnerTag, List<int> playPtn = null)
	{
		AIAttachedTagStopPreprocessOption option = new AIAttachedTagStopPreprocessOption(tagOwner)
		{
			TargetTag = RemoveTag
		};
		AIAttachedTagStopPreprocessOption option2 = new AIAttachedTagStopPreprocessOption(tagOwner)
		{
			TargetTag = executingOwnerTag
		};
		if (situation.Actor.IsAlly)
		{
			field.TagPreprocessContainer.AppendAllyTurnEndStopInfo(option);
			field.TagPreprocessContainer.AppendAllyTurnEndStopInfo(option2);
		}
		else
		{
			field.TagPreprocessContainer.AppendOpponentTurnEndStopInfo(option);
			field.TagPreprocessContainer.AppendOpponentTurnEndStopInfo(option2);
		}
	}
}

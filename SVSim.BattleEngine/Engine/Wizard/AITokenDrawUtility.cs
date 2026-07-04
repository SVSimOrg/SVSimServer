using System.Collections.Generic;

namespace Wizard;

public static class AITokenDrawUtility
{
	public static void ExecuteTokenDraw(AIVirtualCard tagOwner, AISituationInfo situation, AIVirtualField field, List<AITokenInformation> tokenIds, bool isAlly)
	{
		List<AIVirtualCard> list = null;
		for (int i = 0; i < tokenIds.Count; i++)
		{
			int tokenId = tokenIds[i].TokenId;
			AIVirtualCard tokenFromId = field.AI.tokenManager.GetTokenFromId(tokenId, isAlly, field, needsClone: true);
			if (tokenFromId == null)
			{
				AIConsoleUtility.LogError($"AITokenDrawUtility.ExecuteTokenDraw() error!! tokenCard:{tokenId} is null");
				return;
			}
			if (isAlly)
			{
				if (field.AllyHandCards.Count < 9)
				{
					tokenFromId.InitAtDrawToken(tagOwner, situation);
					list = AIParamQuery.AddElementToList(tokenFromId, list);
				}
			}
			else if (field.GetEnemyHandCardList().Count < 9)
			{
				if (field.IsLatestActionField)
				{
					tokenFromId.InitAtDrawToken(tagOwner, situation);
				}
				else
				{
					new EnemyHandVirtualCard(tokenFromId.BaseCard, field).InitAtDrawToken(tagOwner, situation);
				}
				list = AIParamQuery.AddElementToList(tokenFromId, list);
			}
		}
		situation.RegisterOwnDrewCardList(list);
	}
}

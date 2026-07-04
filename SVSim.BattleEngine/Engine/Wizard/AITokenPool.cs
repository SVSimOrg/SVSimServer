using System.Collections.Generic;

namespace Wizard;

public class AITokenPool
{

	private Dictionary<int, AIVirtualCard> tokenPool;

	private BattlePlayerBase owner;

	private int tokenIndex;

	public void AddTokenToPool(EnemyAI ai, int tokenId, bool isChoice)
	{
		if (tokenPool.TryGetValue(tokenId, out var value))
		{
			AIVirtualCard aIVirtualCard = (value.IsAlly ? ai.CurrentVirtualField.AllyClass : ai.CurrentVirtualField.EnemyClass);
			value.IsSelfTurn = aIVirtualCard.IsSelfTurn;
			return;
		}
		int cardIndex = (isChoice ? tokenId : tokenIndex--);
		AIVirtualCard aIVirtualCard2 = new AIVirtualCard(owner.CreateCard(tokenId, cardIndex), ai.CurrentVirtualField);
		aIVirtualCard2.InitializeTags(ai.ParamQuery, null, null);
		tokenPool.Add(tokenId, aIVirtualCard2);
	}

	public AIVirtualCard GetTokenFromPool(int tokenId)
	{
		if (tokenPool.TryGetValue(tokenId, out var value))
		{
			return value;
		}
		AIConsoleUtility.LogError("getTokenFromPool: ID does not exist in the pool! ID: " + tokenId);
		return null;
	}

	public bool IsRegisteredToken(int tokenId)
	{
		return tokenPool.ContainsKey(tokenId);
	}

	public AITokenPool(BattlePlayerBase _owner)
	{
		tokenPool = new Dictionary<int, AIVirtualCard>();
		owner = _owner;
		tokenIndex = -1;
	}
}

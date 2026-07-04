using System.Collections.Generic;

namespace Wizard;

public class AITokenIdCollection
{
	public List<AITokenInformation> AllyTokenIdList;

	public List<AITokenInformation> OpponentTokenIdList;

	public bool HasAllyToken
	{
		get
		{
			if (AllyTokenIdList != null)
			{
				return AllyTokenIdList.Count > 0;
			}
			return false;
		}
	}

	public bool HasOpponentToken
	{
		get
		{
			if (OpponentTokenIdList != null)
			{
				return OpponentTokenIdList.Count > 0;
			}
			return false;
		}
	}

	public bool HasToken
	{
		get
		{
			if (!HasAllyToken)
			{
				return HasOpponentToken;
			}
			return true;
		}
	}

	public AITokenIdCollection()
	{
		AllyTokenIdList = null;
		OpponentTokenIdList = null;
	}

	public void Add(int id, AITokenType tokenType, bool isAllyToken)
	{
		AITokenInformation element = new AITokenInformation(id, tokenType);
		if (isAllyToken)
		{
			AllyTokenIdList = AIParamQuery.AddElementToList(element, AllyTokenIdList);
		}
		else
		{
			OpponentTokenIdList = AIParamQuery.AddElementToList(element, OpponentTokenIdList);
		}
	}

	private void Combine(AITokenIdCollection collection)
	{
		if (collection != null)
		{
			if (collection.HasAllyToken)
			{
				AllyTokenIdList = AIParamQuery.AddRangeToList(collection.AllyTokenIdList, AllyTokenIdList);
			}
			if (collection.HasOpponentToken)
			{
				OpponentTokenIdList = AIParamQuery.AddRangeToList(collection.OpponentTokenIdList, OpponentTokenIdList);
			}
		}
	}

	public static AITokenIdCollection CombineTwoCollection(AITokenIdCollection collection1, AITokenIdCollection collection2)
	{
		if (collection2 == null)
		{
			return collection1;
		}
		if (collection1 == null)
		{
			collection1 = collection2;
		}
		else
		{
			collection1.Combine(collection2);
		}
		return collection1;
	}

	public void MultiplyByRepeatCount(int repeatCount)
	{
		if (repeatCount <= 0)
		{
			if (AllyTokenIdList != null)
			{
				AllyTokenIdList.Clear();
				AllyTokenIdList = null;
			}
			if (OpponentTokenIdList != null)
			{
				OpponentTokenIdList.Clear();
				OpponentTokenIdList = null;
			}
		}
		if (repeatCount <= 1)
		{
			return;
		}
		bool hasAllyToken = HasAllyToken;
		bool hasOpponentToken = HasOpponentToken;
		List<AITokenInformation> list = null;
		List<AITokenInformation> list2 = null;
		for (int i = 0; i < repeatCount; i++)
		{
			if (hasAllyToken)
			{
				list = AIParamQuery.AddRangeToList(AllyTokenIdList, list);
			}
			if (hasOpponentToken)
			{
				list2 = AIParamQuery.AddRangeToList(OpponentTokenIdList, list2);
			}
		}
		AllyTokenIdList = list;
		OpponentTokenIdList = list2;
	}

	public void DrawAllTokenToField(AIVirtualField field, AIVirtualCard owner, AISituationInfo situation)
	{
		if (HasAllyToken)
		{
			AITokenDrawUtility.ExecuteTokenDraw(owner, situation, field, AllyTokenIdList, isAlly: true);
		}
		if (HasOpponentToken)
		{
			AITokenDrawUtility.ExecuteTokenDraw(owner, situation, field, OpponentTokenIdList, isAlly: false);
		}
	}
}
